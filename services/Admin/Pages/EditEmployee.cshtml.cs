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
using System.Collections.Generic;
using System.Linq;

namespace Koasta.Service.Admin.Pages
{
    public class EditEmployeeModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly EmployeeRepository employees;
        private readonly EmployeeRoleRepository roles;

        public string Title { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        public Employee EditingEmployee { get; set; }
        public List<EmployeeRole> Roles { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [MaxLength(200)]
            [Display(Name = "Name")]
            public string EmployeeName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(200)]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [DataType(DataType.Password)]
            [MaxLength(200)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "RoleId")]
            public string RoleId { get; set; }
        }

        public EditEmployeeModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              EmployeeRepository employees,
                              EmployeeRoleRepository roles)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.employees = employees;
            this.roles = roles;
        }

        public async Task<IActionResult> OnGetAsync(int employeeId)
        {
            if (await FetchData(employeeId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Employees");
            }
        }

        public async Task<IActionResult> OnPostAsync(int employeeId)
        {
            if (!await FetchData(employeeId).ConfigureAwait(false))
            {
                return RedirectToPage("/Employees");
            }

            if (!ModelState.IsValid)
            {
                return this.TurboPage();
            }

            var selectedRoleId = int.Parse(Input.RoleId);
            var selectedRole = Roles.Find(r => r.RoleId == selectedRoleId);

            if (selectedRole?.IsMorePrivilegedThanRole(Role) != false)
            {
                return this.TurboPage();
            }

            if (!string.IsNullOrWhiteSpace(Input.Password))
            {
                var result = await userManager.RemovePasswordAsync(EditingEmployee).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "We weren't able to update the password for this account, please contact Koasta support for assistance");
                    return this.Page();
                }

                result = await userManager.AddPasswordAsync(EditingEmployee, Input.Password).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "We weren't able to update the password for this account, please contact Koasta support for assistance");
                    return this.Page();
                }
            }

            return (await employees.UpdateEmployee(new EmployeePatch
            {
                ResourceId = employeeId,
                EmployeeName = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.EmployeeName },
                Username = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.Username },
                RoleId = new PatchOperation<int> { Operation = OperationKind.Update, Value = selectedRoleId },
            })
            .OnSuccess(() => this.RedirectToPage("/Employee", new
            {
                employeeId
            }))
            .OnFailure(() => this.Page())
            .ConfigureAwait(false)).Value;
        }

        private async Task<bool> FetchData(int employeeId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerCompany)
            {
                return false;
            }

            var employee = (await employees.FetchEmployee(employeeId).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Employee found")
                    .OnSuccess(e => e.Value);

            if (employee.IsFailure)
            {
                return false;
            }

            if (employee.Value.CompanyId != Employee.CompanyId && !Role.CanAdministerSystem)
            {
                return false;
            }

            EditingEmployee = employee.Value;
            Title = employee.Value.EmployeeName;

            var allRoles = await roles.FetchEmployeeRoles(0, int.MaxValue)
                .Ensure(t => t.HasValue, "Roles were found")
                .OnSuccess(t => t.Value)
                .OnBoth(t => t.IsSuccess ? t.Value : new List<EmployeeRole>())
                .ConfigureAwait(false);
            Roles = allRoles.Where(r => !r.IsMorePrivilegedThanRole(Role)).ToList();

            Input ??= new InputModel
            {
                EmployeeName = employee.Value.EmployeeName,
                Username = employee.Value.Username
            };

            return true;
        }
    }
}
