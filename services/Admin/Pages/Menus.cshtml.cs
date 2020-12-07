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
    public class MenusModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly MenuRepository menus;

        public string Title { get; set; }
        public List<Menu> Menus { get; set; }
        public int TotalResults { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }
        public int VenueId { get; set; }

        public MenusModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              MenuRepository menus)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.menus = menus;
        }

        public async Task<IActionResult> OnGetAsync(int venueId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);
            ViewData["SubnavVenueId"] = venueId;

            if (!Role.CanWorkWithVenue && Employee.VenueId != venueId)
            {
                return RedirectToPage("/Index");
            }

            var results = (await menus.FetchCountedVenueMenus(venueId, PageNumber, 20).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Menus found")
                    .OnSuccess(e => e.Value)
                    .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Menu> { Data = new List<Menu>(), Count = 0 });
            TotalResults = results.Count;
            Menus = results.Data;
            Title = $"Menus ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);
            VenueId = venueId;

            return Page();
        }
    }
}
