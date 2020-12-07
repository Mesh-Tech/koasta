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
    public class EmployeesModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly EmployeeRepository employees;

        public string Title { get; set; }
        public List<Employee> Employees { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        public int TotalResults { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }

        public EmployeesModel(UserManager<Employee> userManager, 
                              RoleManager<EmployeeRole> roleManager,
                              EmployeeRepository employees)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.employees = employees;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerVenue)
            {
                return RedirectToPage("/Index");
            }

            var task = Role.CanAdministerSystem
                ? employees.FetchCountedEmployees(PageNumber, 20)
                : employees.FetchCountedCompanyEmployees(Employee.CompanyId, PageNumber, 20);

            var results = (await task.ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Employees found")
                .OnSuccess(e => e.Value)
                .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Employee> { Data = new List<Employee>(), Count = 0 });
            TotalResults = results.Count;
            Employees = results.Data;
            Title = $"Employees ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);

            return Page();
        }
    }
}
