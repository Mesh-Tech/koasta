using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using Koasta.Service.Admin.Utils;
using System;

namespace Koasta.Service.Admin.Pages
{
    public class AddCompanyModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly CompanyRepository companies;

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
            public string CompanyName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(8000)]
            [Display(Name = "Address")]
            public string CompanyAddress { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(10)]
            [Display(Name = "Postcode")]
            public string CompanyPostcode { get; set; }

            [DataType(DataType.Text)]
            [MaxLength(100)]
            [Display(Name = "Admin name / department")]
            public string CompanyContact { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [MaxLength(20)]
            [Display(Name = "Contact number")]
            public string CompanyPhone { get; set; }

            [Required]
            [DataType(DataType.EmailAddress)]
            [MaxLength(100)]
            [Display(Name = "E-mail address")]
            public string CompanyEmail { get; set; }
        }

        public AddCompanyModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              CompanyRepository companies)
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

            return (await companies.CreateCompany(new Company
            {
                CompanyName = Input.CompanyName,
                CompanyAddress = Input.CompanyAddress,
                CompanyPostcode = Input.CompanyPostcode,
                CompanyContact = Input.CompanyContact,
                CompanyPhone = Input.CompanyPhone,
                CompanyEmail = Input.CompanyEmail,
                ReferenceCode = Guid.NewGuid().ToString()
            })
            .Ensure(c => c.HasValue, "Company was created")
            .OnSuccess(c => this.RedirectToPage("/Company", new {
                companyId = c.Value
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
