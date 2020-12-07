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
    public class EditTagModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly TagRepository tags;

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
            public string TagName { get; set; }
        }

        public EditTagModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              TagRepository tags)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.tags = tags;
        }

        public async Task<IActionResult> OnGetAsync(int tagId)
        {
            if (await FetchData(tagId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Tags");
            }
        }

        public async Task<IActionResult> OnPostAsync(int tagId)
        {
            if (!ModelState.IsValid)
            {
                await FetchData(tagId).ConfigureAwait(false);
                return this.TurboPage();
            }

            if (!await FetchData(tagId).ConfigureAwait(false))
            {
                return this.TurboPage();
            }

            return (await tags.UpdateTag(new TagPatch
            {
                ResourceId = tagId,
                TagName = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.TagName },
            })
            .OnSuccess(() => this.RedirectToPage("/Tag", new {
                tagId
            }))
            .OnFailure(() => this.Page())
            .ConfigureAwait(false)).Value;
        }

        private async Task<bool> FetchData(int tagId) {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem)
            {
                return false;
            }

            var tag = (await tags.FetchTag(tagId).ConfigureAwait(false))
                        .Ensure(e => e.HasValue, "Tag found")
                        .OnSuccess(e => e.Value);

            if (tag.IsFailure)
            {
                return false;
            }

            Title = tag.Value.TagName;

            Input ??= new InputModel
            {
                TagName = tag.Value.TagName,
            };

            return true;
        }
    }
}
