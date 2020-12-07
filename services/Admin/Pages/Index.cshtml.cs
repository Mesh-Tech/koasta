using CSharpFunctionalExtensions;
using Koasta.Service.Admin.Models;
using Koasta.Service.Admin.Services;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Koasta.Shared.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Koasta.Service.Admin.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SignInManager<Employee> signInManager;
        private readonly CompanyRepository companies;
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly IFeedService feeds;
        private readonly ISettings settings;
        private readonly IDistributedCache cache;
        private readonly AnalyticsRepository analytics;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        public Company Company { get; set; }
        public Feed Feed { get; set; }
        [BindProperty(SupportsGet = false)]
        public string Action { get; set; }
        public string AverageOrderPerVenue { get; set; }
        public string CompanyOrderStatuses { get; set; }
        public ISettings Settings
        {
            get
            {
                return settings;
            }
        }

        public IndexModel(SignInManager<Employee> signInManager,
                          CompanyRepository companies,
                          UserManager<Employee> userManager,
                          RoleManager<EmployeeRole> roleManager,
                          IFeedService feeds,
                          ISettings settings,
                          IDistributedCache cache,
                          AnalyticsRepository analytics)
        {
            this.userManager = userManager;
            this.companies = companies;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.feeds = feeds;
            this.settings = settings;
            this.cache = cache;
            this.analytics = analytics;

            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["HideNavigation"] = !signInManager.IsSignedIn(User);

            if (signInManager.IsSignedIn(User))
            {
                Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
                Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);
                var companyResult = await companies.FetchCompany(Employee.CompanyId)
                    .Ensure(c => c.HasValue, "Company found")
                    .OnSuccess(c => c.Value)
                    .ConfigureAwait(false);

                // We intentionally let this throw on failure as this is the index page, so we have nowhere safe to redirect to.
                Company = companyResult.Value;
                Feed = await feeds.GetFeed().ConfigureAwait(false);

                var averageOrderResult = await analytics.GetAverageOrderAmount(Company.CompanyId)
                    .Ensure(r => r.HasValue, "Results found")
                    .OnSuccess(r => r.Value)
                    .ConfigureAwait(false);

                if (averageOrderResult.IsSuccess)
                {
                    var data = new BarChartData {
                        Labels = new List<string>(),
                        Series = new List<List<double>> { new List<double>() }
                    };

                    averageOrderResult.Value.ForEach(result => {
                        data.Labels.Add(result.VenueId.ToString());
                        data.Series[0].Add( (double) result.Average );
                    });

                    AverageOrderPerVenue = JsonConvert.SerializeObject(data, jsonSerializerSettings);
                }

                var companyOrderStatuses = await analytics.GetCompanyOrderStatuses(Company.CompanyId)
                    .Ensure(r => r.HasValue, "Results found")
                    .OnSuccess(r => r.Value)
                    .ConfigureAwait(false);

                if (companyOrderStatuses.IsSuccess)
                {
                    var data = new BarChartDescriptiveData
                    {
                        Labels = new List<string>(),
                        Series = new List<List<BarChartDesciptiveElem>>()
                    };

                    for(var i = 0; i < 3; i++)
                    {
                        data.Series.Add(new List<BarChartDesciptiveElem>());
                    }

                    companyOrderStatuses.Value.ForEach(result =>
                    {
                        data.Labels.Add(result.VenueId.ToString());

                        data.Series[0].Add(new BarChartDesciptiveElem
                        {
                            Meta = "Incomplete",
                            Value = result.Incomplete
                        });

                        data.Series[1].Add(new BarChartDesciptiveElem
                        {
                            Meta = "Complete",
                            Value = result.Complete
                        });

                        data.Series[2].Add(new BarChartDesciptiveElem
                        {
                            Meta = "Failed",
                            Value = result.Failed
                        });
                    });

                    CompanyOrderStatuses = JsonConvert.SerializeObject(data, jsonSerializerSettings);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.Equals(Action, "square-connect", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Index");
            }

            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerCompany)
            {
                return RedirectToPage("/Index");
            }

            var verificationKey = Guid.NewGuid().ToString("N");
            await cache.SetStringAsync($"company-square-verify-{verificationKey}", Employee.EmployeeId.ToString(), new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5))).ConfigureAwait(false);

            var sessionParam = Settings.Connection.SquarePaymentsSandbox ? "" : "&session=false";
            var baseUrl = Settings.Connection.SquarePaymentsSandbox ? "https://connect.squareupsandbox.com" : "https://connect.squareup.com";

            return Redirect($"{baseUrl}/oauth2/authorize?client_id={Settings.Connection.SquareAppId}&scope=MERCHANT_PROFILE_READ+PAYMENTS_WRITE_ADDITIONAL_RECIPIENTS+PAYMENTS_WRITE+PAYMENTS_READ+MERCHANT_PROFILE_WRITE{sessionParam}&state={verificationKey}");
        }
    }
}
