using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Koasta.Shared.Types;
using System;
using System.Linq;

namespace Koasta.Service.Admin.Pages
{
    public class QueryResult {
        public string Title { get; set; }
        public string Href { get; set; }
    }
    public class QueriesModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly VenueRepository venues;

        public string Title { get; set; }
        public List<QueryResult> Results { get; set; }
        public int TotalResults { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; }

        public QueriesModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              VenueRepository venues)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.venues = venues;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Title = "Queries";

            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem)
            {
                return RedirectToPage("/Index");
            }

            if (string.IsNullOrWhiteSpace(Query))
            {
                return Page();
            }

            if (Query.Equals("venues-pending-verify", StringComparison.OrdinalIgnoreCase))
            {
                await ExecuteVenuesPendingVerifyQuery().ConfigureAwait(false);
            }
            else if (Query.Equals("venues-rejected", StringComparison.OrdinalIgnoreCase))
            {
                await ExecuteRejectedVenuesQuery().ConfigureAwait(false);
            }

            return Page();
        }

        private async Task ExecuteVenuesPendingVerifyQuery()
        {
            var results = (await venues.FetchCountedPendingVerifyVenues(PageNumber, 20).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Venues found")
                    .OnSuccess(e => e.Value)
                    .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Venue> { Data = new List<Venue>(), Count = 0 });
            TotalResults = results.Count;
            Results = results.Data.Select(v => new QueryResult {
                Title = v.VenueName,
                Href = $"/venue/{v.VenueId}"
            }).ToList();
            Title = $"Pending venues ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);
        }

        private async Task ExecuteRejectedVenuesQuery()
        {
            var results = (await venues.FetchCountedRejectedVenues(PageNumber, 20).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Venues found")
                    .OnSuccess(e => e.Value)
                    .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Venue> { Data = new List<Venue>(), Count = 0 });
            TotalResults = results.Count;
            Results = results.Data.Select(v => new QueryResult
            {
                Title = v.VenueName,
                Href = $"/venue/{v.VenueId}"
            }).ToList();
            Title = $"Rejected venues ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);
        }
    }
}
