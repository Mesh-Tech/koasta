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
    public class TagModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly TagRepository tags;

        public string Title { get; set; }
        public Tag Tag { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = false)]
        public string Action { get; set; }

        public TagModel(UserManager<Employee> userManager,
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
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int tagId)
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

            var result = await tags.DropTag(tagId).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return RedirectToPage("/Tags");
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<bool> FetchData(int tagId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem) {
                return false;
            }

            var result = (await tags.FetchTag(tagId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Tag found")
                                    .OnSuccess(e => e.Value);

            Tag = result.IsSuccess ? result.Value : null;
            Title = Tag.TagName;
            return result.IsSuccess;
        }
    }
}
