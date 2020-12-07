using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using System;

namespace Koasta.Service.Admin.Pages
{
    public class MenuModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly MenuRepository menus;
        private readonly VenueRepository venues;

        public string Title { get; set; }
        public FullMenu Menu { get; set; }
        public string MenuType { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = false)]
        public string Action { get; set; }

        public MenuModel(UserManager<Employee> userManager,
                        RoleManager<EmployeeRole> roleManager,
                        VenueRepository venues,
                        MenuRepository menus)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.menus = menus;
            this.venues = venues;
        }

        public async Task<IActionResult> OnGetAsync(int menuId)
        {
            if (await FetchData(menuId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int menuId)
        {
            if (!string.Equals(Action, "delete", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Index");
            }

            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerVenue)
            {
                return RedirectToPage("/Index");
            }

            var getResult = (await menus.FetchMenu(menuId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Menu found")
                                    .OnSuccess(e => e.Value);

            if (!getResult.IsSuccess)
            {
                return RedirectToPage("/Index");
            }

            var result = await menus.DropMenu(menuId).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return RedirectToPage("/Menus", new
                {
                    venueId = getResult.Value.VenueId
                });
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<bool> FetchData(int menuId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanWorkWithVenue)
            {
                return false;
            }

            var result = (await menus.FetchFullMenu(menuId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Menu found")
                                    .OnSuccess(e => e.Value);

            Menu = result.IsSuccess ? result.Value : null;

            var menuVenue = (await venues.FetchVenue(Menu.VenueId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Venue found")
                                    .OnSuccess(e => e.Value);

            if (menuVenue.IsFailure)
            {
                return false;
            }

            if (!Role.CanAdministerSystem && menuVenue.Value.CompanyId != Employee.CompanyId)
            {
                return false;
            }

            ViewData["SubnavVenueId"] = Menu.VenueId;
            Title = Menu.MenuName;
            return result.IsSuccess;
        }
    }
}
