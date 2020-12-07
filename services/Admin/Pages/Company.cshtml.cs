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
    public class CompanyModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly CompanyRepository companies;

        public string Title { get; set; }
        public Company Company { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = false)]
        public string Action { get; set; }

        public CompanyModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              CompanyRepository companies)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.companies = companies;
        }

        public async Task<IActionResult> OnGetAsync(int companyId)
        {
            if (await FetchData(companyId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int companyId)
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

            var result = await companies.DropCompany(companyId).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return RedirectToPage("/Companies");
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<bool> FetchData(int companyId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem && Employee.CompanyId != companyId) {
                return false;
            }

            if (Role.CanAdministerCompany || Role.CanWorkWithCompany)
            {
                var result = (await companies.FetchCompany(companyId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Company found")
                                    .OnSuccess(e => e.Value);

                Company = result.IsSuccess ? result.Value : null;
                ViewData["SubnavCompanyId"] = Company?.CompanyId;
                Title = Company.CompanyName;
                return result.IsSuccess;
            }

            return false;
        }
    }
}
