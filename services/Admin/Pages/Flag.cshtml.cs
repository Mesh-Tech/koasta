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
    public class FlagModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly FeatureFlagRepository flags;

        public string Title { get; set; }
        public FeatureFlag Flag { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = false)]
        public string Action { get; set; }

        public FlagModel(UserManager<Employee> userManager,
                        RoleManager<EmployeeRole> roleManager,
                        FeatureFlagRepository flags)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.flags = flags;
        }

        public async Task<IActionResult> OnGetAsync(int flagId)
        {
            if (await FetchData(flagId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int flagId)
        {
            if (!string.Equals(Action, "delete", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Index");
            }

            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem)
            {
                return RedirectToPage("/Index");
            }

            var result = await flags.DropFeatureFlag(flagId).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return RedirectToPage("/Flags");
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<bool> FetchData(int flagId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem) {
                return false;
            }

            var result = (await flags.FetchFeatureFlag(flagId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Flag found")
                                    .OnSuccess(e => e.Value);

            Flag = result.IsSuccess ? result.Value : null;
            Title = Flag.Name;
            return result.IsSuccess;
        }
    }
}
