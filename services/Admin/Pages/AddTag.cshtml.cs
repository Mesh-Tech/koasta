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
    public class AddTagModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly TagRepository companies;

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

        public AddTagModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              TagRepository companies)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.companies = companies;
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

            return (await companies.CreateTag(new Tag
            {
                TagName = Input.TagName
            })
            .Ensure(c => c.HasValue, "Tag was created")
            .OnSuccess(c => this.RedirectToPage("/Tag", new {
                tagId = c.Value
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
