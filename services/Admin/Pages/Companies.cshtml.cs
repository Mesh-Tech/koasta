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
    public class CompaniesModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly CompanyRepository companies;

        public string Title { get; set; }
        public List<Company> Companies { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        public int TotalResults { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }

        public CompaniesModel(UserManager<Employee> userManager, 
                              RoleManager<EmployeeRole> roleManager,
                              CompanyRepository companies)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.companies = companies;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem)
            {
                return RedirectToPage("/Index");
            }

            var results = (await companies.FetchCountedCompanies(PageNumber, 20).ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Companies found")
                .OnSuccess(e => e.Value)
                .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Company> { Data = new List<Company>(), Count = 0 });
            TotalResults = results.Count;
            Companies = results.Data;
            Title = $"Companies ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);

            return Page();
        }
    }
}
