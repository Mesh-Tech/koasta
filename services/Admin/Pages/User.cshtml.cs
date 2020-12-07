using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Koasta.Service.Admin.Pages
{
    public class UserModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly UserRepository users;

        public string Title { get; set; }
        public User SelectedUser { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }

        public UserModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              UserRepository users)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.users = users;
        }

        public async Task<IActionResult> OnGetAsync(int userId)
        {
            if (await FetchData(userId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<bool> FetchData(int userId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem) {
                return false;
            }

            var result = (await users.FetchUser(userId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "SelectedUser found")
                                    .OnSuccess(e => e.Value);

            SelectedUser = result.IsSuccess ? result.Value : null;
            Title = $"User ID {SelectedUser.UserId}";
            return result.IsSuccess;
        }
    }
}
