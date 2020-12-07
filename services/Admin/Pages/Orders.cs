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
    public class OrdersModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly OrderRepository venues;

        public string Title { get; set; }
        public List<Order> Orders { get; set; }
        public int TotalResults { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }

        public OrdersModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              OrderRepository venues)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.venues = venues;
        }

        public async Task<IActionResult> OnGetAsync(int? venueId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);
            if (venueId != null)
            {
                ViewData["SubnavVenueId"] = venueId.Value;
            }

            if (!Role.CanWorkWithVenue && Employee.VenueId != venueId)
            {
                return RedirectToPage("/Index");
            }

            var task = venueId == null
                ? venues.FetchCountedCompleteOrders(PageNumber, 20)
                : venues.FetchCountedCompleteVenueOrders(venueId.Value, PageNumber, 20);

            var results = (await task.ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Orders found")
                    .OnSuccess(e => e.Value)
                    .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Order> { Data = new List<Order>(), Count = 0 });
            TotalResults = results.Count;
            Orders = results.Data;
            Title = $"Orders ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);

            return Page();
        }
    }
}
