using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using Koasta.Service.Admin.Utils;

namespace Koasta.Service.Admin.Pages
{
    public class AddFlagModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly FeatureFlagRepository flags;

        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [MaxLength(100)]
            [Display(Name = "Name")]
            public string FlagName { get; set; }
            [DataType(DataType.Text)]
            [MaxLength(8000)]
            [Display(Name = "Description")]
            public string FlagDescription { get; set; }
            [Required]
            [Range(typeof(bool), "false", "true", ErrorMessage = "Status is required.")]
            [Display(Name = "Status")]
            public bool FlagValue { get; set; }
        }

        public AddFlagModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              FeatureFlagRepository flags)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.flags = flags;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await FetchData().ConfigureAwait(false);

            if (await FetchData().ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await FetchData().ConfigureAwait(false);
                return this.TurboPage();
            }

            if (!await FetchData().ConfigureAwait(false))
            {
                return RedirectToPage("/Index");
            }

            return (await flags.CreateFeatureFlag(new FeatureFlag
            {
                Name = Input.FlagName,
                Description = Input.FlagDescription,
                Value = Input.FlagValue
            })
            .Ensure(c => c.HasValue, "Flag was created")
            .OnSuccess(c => this.RedirectToPage("/Flag", new {
                flagId = c.Value
            }))
            .OnFailure(() => this.Page())
            .ConfigureAwait(false)).Value;
        }

        private async Task<bool> FetchData() {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            return Role.CanAdministerSystem;
        }
    }
}
