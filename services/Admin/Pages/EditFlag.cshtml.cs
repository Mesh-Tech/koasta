using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using Koasta.Shared.PatchModels;
using Koasta.Service.Admin.Utils;

namespace Koasta.Service.Admin.Pages
{
    public class EditFlagModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly FeatureFlagRepository flags;

        public string Title { get; set; }
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

        public EditFlagModel(UserManager<Employee> userManager,
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
                return RedirectToPage("/Flags");
            }
        }

        public async Task<IActionResult> OnPostAsync(int flagId)
        {
            if (!ModelState.IsValid)
            {
                await FetchData(flagId).ConfigureAwait(false);
                return this.TurboPage();
            }

            if (!await FetchData(flagId).ConfigureAwait(false))
            {
                return this.TurboPage();
            }

            return (await flags.UpdateFeatureFlag(new FeatureFlagPatch
            {
                ResourceId = flagId,
                Name = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.FlagName },
                Description = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.FlagDescription },
                Value = new PatchOperation<bool> { Operation = OperationKind.Update, Value = Input.FlagValue },
            })
            .OnSuccess(() => this.RedirectToPage("/Flag", new {
                flagId
            }))
            .OnFailure(() => this.Page())
            .ConfigureAwait(false)).Value;
        }

        private async Task<bool> FetchData(int flagId) {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem)
            {
                return false;
            }

            var flag = (await flags.FetchFeatureFlag(flagId).ConfigureAwait(false))
                        .Ensure(e => e.HasValue, "Flag found")
                        .OnSuccess(e => e.Value);

            if (flag.IsFailure)
            {
                return false;
            }

            Title = flag.Value.Name;

            Input ??= new InputModel
            {
                FlagName = flag.Value.Name,
                FlagDescription = flag.Value.Description,
                FlagValue = flag.Value.Value
            };

            return true;
        }
    }
}
