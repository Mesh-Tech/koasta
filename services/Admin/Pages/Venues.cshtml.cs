using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Koasta.Shared.Types;

namespace Koasta.Service.Admin.Pages
{
    public class VenuesModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly VenueRepository venues;

        public string Title { get; set; }
        public List<Venue> Venues { get; set; }
        public int TotalResults { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }
        [BindProperty(SupportsGet = true)]
        public string Query { get; set; }

        public VenuesModel(UserManager<Employee> userManager, 
                              RoleManager<EmployeeRole> roleManager,
                              VenueRepository venues)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.venues = venues;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem)
            {
                return RedirectToPage("/Index");
            }

            var task = string.IsNullOrWhiteSpace(Query)
                ? venues.FetchCountedVenues(PageNumber, 20)
                : venues.FetchCountedQueriedVenues(Query, PageNumber, 20);

            var results = (await task.ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Venues found")
                    .OnSuccess(e => e.Value)
                    .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Venue> { Data = new List<Venue>(), Count = 0 });
            TotalResults = results.Count;
            Venues = results.Data;
            Title = $"Venues ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);

            return Page();
        }
    }
}