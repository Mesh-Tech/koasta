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
    public class FlagsModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly FeatureFlagRepository flags;

        public string Title { get; set; }
        public List<FeatureFlag> Flags { get; set; }
        public int TotalResults { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }

        public FlagsModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              FeatureFlagRepository flags)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.flags = flags;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem)
            {
                return RedirectToPage("/Index");
            }

            var results = (await flags.FetchCountedFeatureFlags(PageNumber, 20).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Flags found")
                    .OnSuccess(e => e.Value)
                    .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<FeatureFlag> { Data = new List<FeatureFlag>(), Count = 0 });
            TotalResults = results.Count;
            Flags = results.Data;
            Title = $"Feature Flags ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);

            return Page();
        }
    }
}
