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
    public class EmployeeModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly EmployeeRepository employees;

        public string Title { get; set; }
        public Employee Employee { get; set; }
        public Employee SelectedEmployee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = false)]
        public string Action { get; set; }

        public EmployeeModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              EmployeeRepository employees)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.employees = employees;
        }

        public async Task<IActionResult> OnGetAsync(int employeeId)
        {
            if (await FetchData(employeeId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int employeeId)
        {
            if (!string.Equals(Action, "delete", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Index");
            }

            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerCompany)
            {
                return RedirectToPage("/Index");
            }

            var getResult = (await employees.FetchEmployee(employeeId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Employee found")
                                    .OnSuccess(e => e.Value);

            if (!getResult.IsSuccess || (!Role.CanAdministerSystem && getResult.Value.CompanyId != Employee.CompanyId))
            {
                return RedirectToPage("/Index");
            }

            var result = await employees.DropEmployee(employeeId).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return RedirectToPage("/Employees");
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<bool> FetchData(int employeeId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (Role.CanWorkWithCompany)
            {
                var result = (await employees.FetchEmployee(employeeId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Employee found")
                                    .OnSuccess(e => e.Value);

                SelectedEmployee = result.IsSuccess ? result.Value : null;

                if (!Role.CanAdministerSystem && SelectedEmployee.CompanyId != Employee.CompanyId)
                {
                    return false;
                }

                Title = SelectedEmployee.EmployeeName;
                return result.IsSuccess;
            }

            return false;
        }
    }
}
