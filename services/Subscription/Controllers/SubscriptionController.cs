using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Models;
using Koasta.Service.SubscriptionService.Models;
using Koasta.Shared.Billing;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Koasta.Service.SubscriptionService.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/subscription")]
    public class SubscriptionController : Controller
    {
        private readonly SubscriptionPackageRepository packages;
        private readonly SubscriptionPlanRepository plans;
        private readonly CompanyRepository companies;
        private readonly IBillingManager billingManager;
        private readonly ILogger logger;

        public SubscriptionController(SubscriptionPackageRepository packages, SubscriptionPlanRepository plans,
                                      CompanyRepository companies, IBillingManager billingManager,
                                      ILoggerFactory logger)
        {
            this.packages = packages;
            this.plans = plans;
            this.companies = companies;
            this.billingManager = billingManager;
            this.logger = logger.CreateLogger("SubscriptionController");
        }

        [HttpGet]
        [Route("packages")]
        [ActionName("fetch_packages")]
        [ProducesResponseType(typeof(List<SubscriptionPackage>), 200)]
        public async Task<IActionResult> GetPackages()
        {
            return await packages.FetchSubscriptionPackages(0, 2000)
              .Ensure(p => p.HasValue, "Packages exist")
              .OnBoth(p => p.IsFailure ? StatusCode(404, "") : StatusCode(200, p.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("packages/{packageId}")]
        [ActionName("fetch_package")]
        [ProducesResponseType(typeof(DtoSubscriptionPackage), 200)]
        public async Task<IActionResult> GetPackage([FromRoute(Name = "packageId")] int packageId)
        {
            return await packages.FetchSubscriptionPackage(packageId)
              .Ensure(p => p.HasValue, "Package exists")
              .OnSuccess(p => MergePackageWithStripeInfo(p.Value))
              .OnBoth(p => p.IsFailure ? StatusCode(404, "") : StatusCode(200, p.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("plans/{companyId}")]
        [ActionName("fetch_plan")]
        [ProducesResponseType(typeof(DtoSubscriptionPlan), 200)]
        public async Task<IActionResult> GetPlan([FromRoute(Name = "companyId")] int companyId)
        {
            return await plans.FetchCompanySubscriptionPlan(companyId)
              .Ensure(p => p.HasValue, "Plan exists")
              .OnSuccess(p => MergePlanWithStripeInfo(p.Value))
              .OnBoth(p => p.IsFailure ? StatusCode(404, "") : StatusCode(200, p.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("plans/my-plan")]
        [ActionName("fetch_my_plan")]
        [ProducesResponseType(typeof(DtoSubscriptionPlan), 200)]
        public async Task<IActionResult> GetMyPlan()
        {
            var companyId = this.GetAuthContext().Employee.Value.CompanyId;
            return await plans.FetchCompanySubscriptionPlan(companyId)
              .Ensure(p => p.HasValue, "Plan exists")
              .OnSuccess(p => MergePlanWithStripeInfo(p.Value))
              .OnBoth(p => p.IsFailure ? StatusCode(404, "") : StatusCode(200, p.Value))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [Route("packages")]
        [ActionName("create_package")]
        public async Task<IActionResult> CreatePackage([FromBody] DtoNewSubscriptionPackage package)
        {
            return await RegisterPackageWithStripe(package)
              .OnSuccess(p => packages.CreateSubscriptionPackage(p))
              .Ensure(p => p.HasValue, "Package exists")
              .OnBoth(p => p.IsFailure ? StatusCode(404) : StatusCode(201))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [Route("plans/{companyId}")]
        [ActionName("create_plan")]
        public async Task<IActionResult> CreateEmptyPlan([FromRoute(Name = "companyId")] int companyId)
        {
            var plan = new SubscriptionPlan
            {
                CompanyId = companyId
            };
            return await plans.CreateSubscriptionPlan(plan)
              .OnBoth(p => p.IsFailure ? StatusCode(404) : StatusCode(201))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [Route("plans")]
        [ActionName("create_full_plan")]
        public async Task<IActionResult> CreatePlan([FromBody] DtoNewSubscriptionPlan plan)
        {
            return await companies.FetchCompany(plan.CompanyId)
              .Ensure(company => company.HasValue, "Employee's company was found")
              .OnSuccess(company => RegisterPlanWithStripe(plan.Packages, company.Value.ExternalAccountId))
              .OnSuccess(subscriptionId => plans.CreateSubscriptionPlan(new SubscriptionPlan
              {
                  CompanyId = plan.CompanyId,
                  ExternalPlanId = subscriptionId
              }))
              .OnSuccess(planId => packages.ReplaceSubscriptionPackageEntriesForPlanId(planId.Value, plan.Packages))
              .OnBoth(p => p.IsFailure ? StatusCode(404) : StatusCode(201))
              .ConfigureAwait(false);
        }

        [HttpPut]
        [Route("plans/{planId}")]
        [ActionName("update_plan")]
        public async Task<IActionResult> ReplacePlan([FromRoute(Name = "planId")] int planId, [FromBody] DtoNewSubscriptionPlan updatedPlan)
        {
            Company existingCompany = null;
            return await companies.FetchCompany(updatedPlan.CompanyId)
              .Ensure(company => company.HasValue, "Employee's company was found")
              .OnSuccess(company =>
              {
                  existingCompany = company.Value;
                  return plans.FetchSubscriptionPlan(planId);
              })
              .Ensure(plan => plan.HasValue, "Company plan was found")
              .OnSuccess(plan => UpdatePlanWithStripe(updatedPlan.Packages, existingCompany.ExternalAccountId, plan.Value.ExternalPlanId))
              .OnSuccess(() => packages.ReplaceSubscriptionPackageEntriesForPlanId(planId, updatedPlan.Packages))
              .OnBoth(p => p.IsFailure ? StatusCode(404) : StatusCode(200))
              .ConfigureAwait(false);
        }

        [HttpDelete]
        [Route("plans/{planId}")]
        [ActionName("delete_plan")]
        public async Task<IActionResult> DropPlan([FromRoute(Name = "planId")] int planId)
        {
            return await plans.FetchSubscriptionPlan(planId)
              .Ensure(plan => plan.HasValue, "Plan was found")
              .OnSuccess(async plan =>
              {
                  await billingManager.DeleteSubscription(plan.Value.ExternalPlanId).ConfigureAwait(false);
                  await plans.DropSubscriptionPlan(plan.Value.PlanId).ConfigureAwait(false);
              })
              .OnBoth(p => p.IsFailure ? StatusCode(404) : StatusCode(200))
              .ConfigureAwait(false);
        }

        private async Task<Result<DtoSubscriptionPackage>> MergePackageWithStripeInfo(SubscriptionPackage package)
        {
            try
            {
                var result = await billingManager.GetSubscriptionPackage(package.ExternalPackageId).ConfigureAwait(false);
                if (result == null)
                {
                    return Result.Fail<DtoSubscriptionPackage>("Failed to fetch package info from Stripe, null response.");
                }

                var sPackage = new DtoSubscriptionPackage
                {
                    PackageId = package.PackageId,
                    PackageName = package.PackageName,
                    MonthlyAmount = result.MonthlyAmount,
                    Identifier = result.Identifier,
                };

                return Result.Ok<DtoSubscriptionPackage>(sPackage);
            }
            catch (Exception)
            {
                return Result.Fail<DtoSubscriptionPackage>("Failed to fetch package info from Stripe");
            }
        }

        private async Task<Result<DtoSubscriptionPackage>> MergePackageWithStripeInfo(FullSubscriptionPackage package)
        {
            try
            {
                var result = await billingManager.GetSubscriptionPackage(package.ExternalPackageId).ConfigureAwait(false);
                if (result == null)
                {
                    return Result.Fail<DtoSubscriptionPackage>("Failed to fetch package info from Stripe, null response.");
                }

                var sPackage = new DtoSubscriptionPackage
                {
                    PackageId = package.PackageId,
                    PackageName = package.PackageName,
                    MonthlyAmount = result.MonthlyAmount,
                    Identifier = result.Identifier,
                };

                return Result.Ok<DtoSubscriptionPackage>(sPackage);
            }
            catch (Exception)
            {
                return Result.Fail<DtoSubscriptionPackage>("Failed to fetch package info from Stripe");
            }
        }

        private async Task<Result<DtoSubscriptionPlan>> MergePlanWithStripeInfo(FullSubscriptionPlan plan)
        {
            var res = (await Task.WhenAll(plan.Packages.Select(async p => await MergePackageWithStripeInfo(p).ConfigureAwait(false))).ConfigureAwait(false))
              .ToList();

            if (res.Any(p => p.IsFailure))
            {
                return Result.Fail<DtoSubscriptionPlan>("Failed to fetch package info from Stripe");
            }

            var packages = res.Select(p => p.Value).ToList();

            return Result.Ok<DtoSubscriptionPlan>(new DtoSubscriptionPlan
            {
                PlanId = plan.PlanId,
                CompanyId = plan.CompanyId,
                ExternalPlanId = plan.ExternalPlanId,
                Packages = packages,
            });
        }

        private async Task<Result<SubscriptionPackage>> RegisterPackageWithStripe(DtoNewSubscriptionPackage package)
        {
            var billingPackage = new BillingSubscriptionPackage
            {
                Name = package.PackageName,
                MonthlyAmount = package.MonthlyAmount,
                Identifier = package.Identifier,
            };

            try
            {
                var id = await billingManager.AddSubscriptionPackage(billingPackage).ConfigureAwait(false);

                return Result.Ok<SubscriptionPackage>(new SubscriptionPackage
                {
                    PackageName = package.PackageName,
                    ExternalPackageId = id,
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to register package with Stripe");
                return Result.Fail<SubscriptionPackage>("Failed to register package with Stripe");
            }
        }

        private async Task<Result<string>> RegisterPlanWithStripe(List<int> packageIds, string customerId)
        {
            return await packages.FetchSubscriptionPackagesFromIds(packageIds)
              .Ensure(p => p.HasValue, "Packages found")
              .OnSuccess(p => p.Value.Select(pkg => pkg.ExternalPackageId).ToList())
              .OnSuccess(p => billingManager.CreateSubscription(customerId, p))
              .ConfigureAwait(false);
        }

        private async Task<Result> UpdatePlanWithStripe(List<int> packageIds, string customerId, string subscriptionId)
        {
            return await packages.FetchSubscriptionPackagesFromIds(packageIds)
              .Ensure(p => p.HasValue, "Packages found")
              .OnSuccess(p => p.Value.Select(pkg => pkg.ExternalPackageId).ToList())
              .OnSuccess(p => billingManager.UpdateSubscription(customerId, subscriptionId, p))
              .ConfigureAwait(false);
        }
    }
}
