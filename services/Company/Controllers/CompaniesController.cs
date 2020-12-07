using Microsoft.AspNetCore.Mvc;
using Koasta.Shared.Middleware;
using System.Collections.Generic;
using System.Threading.Tasks;
using Koasta.Shared.Models;
using Koasta.Shared.Database;
using CSharpFunctionalExtensions;
using Koasta.Service.CompanyService.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Koasta.Shared.Billing;
using System;
using Koasta.Shared.PatchModels;
using Microsoft.Extensions.Logging;

namespace Koasta.Service.CompanyService.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/companies")]
    public class CompaniesController : Controller
    {
        private readonly CompanyRepository companies;
        private readonly IBillingManager billingManager;
        private readonly ILogger logger;

        public CompaniesController(CompanyRepository companies, IBillingManager billingManager, ILoggerFactory logger)
        {
            this.companies = companies;
            this.billingManager = billingManager;
            this.logger = logger.CreateLogger("CompaniesController");
        }

        [HttpGet]
        [ActionName("list_companies")]
        [ProducesResponseType(typeof(List<Company>), 200)]
        public async Task<IActionResult> GetCompanies([FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await companies.FetchCompanies(page, count)
              .OnSuccess(c => c.HasNoValue ? new List<Company>() : c.Value)
              .OnBoth(c => c.IsFailure
                ? StatusCode(500, "")
                : StatusCode(200, c.Value)
              )
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_company")]
        [Route("{companyId}")]
        [ProducesResponseType(typeof(Company), 200)]
        public async Task<IActionResult> GetCompany([FromRoute(Name = "companyId")] int companyId)
        {
            return await companies.FetchCompany(companyId)
              .Ensure(c => c.HasValue, "Company does exist")
              .OnBoth(c => c.IsFailure
                ? StatusCode(404, "")
                : StatusCode(200, c.Value.Value)
              )
              .ConfigureAwait(false);
        }

        [HttpDelete]
        [Route("{companyId}")]
        public async Task<IActionResult> DeleteCompany([FromRoute(Name = "companyId")] int companyId)
        {
            return await companies.DropCompany(companyId)
              .OnBoth(c => c.IsFailure
                ? StatusCode(404)
                : StatusCode(200)
              )
              .ConfigureAwait(false);
        }

        [HttpPost]
        [ActionName("create_company")]
        public async Task<IActionResult> CreateCompany([FromBody] DtoCreateCompanyRequest request)
        {
            if (ModelState.ValidationState != ModelValidationState.Valid)
            {
                return BadRequest();
            }

            return await companies.CreateCompany(new Company
            {
                CompanyName = request.CompanyName,
                CompanyAddress = request.CompanyAddress,
                CompanyPostcode = request.CompanyPostcode,
                CompanyContact = request.CompanyContact,
                CompanyPhone = request.CompanyPhone,
                CompanyEmail = request.CompanyEmail
            })
            .Ensure(companyId => companyId.HasValue, "Company created")
            .OnSuccess(companyId => companies.FetchCompany(companyId.Value))
            .Ensure(company => company.HasValue, "Company exists")
            .OnSuccess(company => RegisterBillingAccount(company.Value, request))
            .OnBoth(result => result.IsFailure ? StatusCode(500) : StatusCode(201))
            .ConfigureAwait(false);
        }

        [HttpPut]
        [ActionName("update_company")]
        [Route("{companyId}")]
        public async Task<IActionResult> ReplaceCompany([FromBody] DtoUpdateCompanyRequest request, [FromRoute(Name = "companyId")] int companyId)
        {
            if (ModelState.ValidationState != ModelValidationState.Valid)
            {
                return BadRequest();
            }

            var company = new Company
            {
                CompanyId = companyId,
                CompanyName = request.CompanyName,
                CompanyAddress = request.CompanyAddress,
                CompanyPostcode = request.CompanyPostcode,
                CompanyContact = request.CompanyContact,
                CompanyPhone = request.CompanyPhone,
                CompanyEmail = request.CompanyEmail
            };

            return await companies.ReplaceCompany(company)
            .OnSuccess(() => UpdateBillingAccount(company, request))
            .OnBoth(result => result.IsFailure ? StatusCode(500) : StatusCode(200))
            .ConfigureAwait(false);
        }

        private async Task<Result<Company>> RegisterBillingAccount(Company company, DtoCreateCompanyRequest request)
        {
            string accountId;
            try
            {
                accountId = await billingManager.RegisterBillingAccount(company, new BillingAccountDetails
                {
                    IsBusinessAccount = request.BankAccountIsBusiness,
                    AccountNumber = request.AccountNumber,
                    SortCode = request.SortCode,
                    NameOnAccount = request.NameOnAccount
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result.Fail<Company>($"Error occurred creating account: {ex}");
            }

            company.ExternalAccountId = accountId;

            return await companies.UpdateCompany(new CompanyPatch
            {
                ResourceId = company.CompanyId,
                ExternalAccountId = new PatchOperation<string> { Operation = OperationKind.Update, Value = accountId }
            })
            .OnSuccess(() => company)
            .ConfigureAwait(false);
        }

        private async Task<Result> UpdateBillingAccount(Company company, DtoUpdateCompanyRequest request)
        {
            if (request.AccountNumber == null || request.NameOnAccount == null || request.SortCode == null)
            {
                return Result.Ok();
            }

            try
            {
                await billingManager.UpdateBillingAccount(company, new BillingAccountDetails
                {
                    IsBusinessAccount = request.BankAccountIsBusiness,
                    AccountNumber = request.AccountNumber,
                    SortCode = request.SortCode,
                    NameOnAccount = request.NameOnAccount
                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update billing account");
                return Result.Fail($"Error occurred creating account: {ex}");
            }

            return Result.Ok();
        }
    }
}
