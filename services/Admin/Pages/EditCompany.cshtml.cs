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
    public class EditCompanyModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly CompanyRepository companies;

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

        public EditCompanyModel(UserManager<Employee> userManager,
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
                return RedirectToPage("/Companies");
            }
        }

        public async Task<IActionResult> OnPostAsync(int companyId)
        {
            if (!ModelState.IsValid)
            {
                await FetchData(companyId).ConfigureAwait(false);
                return this.TurboPage();
            }

            return (await companies.UpdateCompany(new CompanyPatch
            {
                ResourceId = companyId,
                CompanyName = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.CompanyName },
                CompanyAddress = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.CompanyAddress },
                CompanyPostcode = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.CompanyPostcode },
                CompanyContact = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.CompanyContact },
                CompanyPhone = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.CompanyPhone },
                CompanyEmail = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.CompanyEmail },
            })
            .OnSuccess(() => this.RedirectToPage("/Company", new {
                companyId
            }))
            .OnFailure(() => this.Page())
            .ConfigureAwait(false)).Value;
        }

        private async Task<bool> FetchData(int companyId) {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem && Employee.CompanyId != companyId)
            {
                return false;
            }

            if (Role.CanAdministerCompany || Role.CanWorkWithCompany)
            {
                var company = (await companies.FetchCompany(companyId).ConfigureAwait(false))
                        .Ensure(e => e.HasValue, "Company found")
                        .OnSuccess(e => e.Value);

                if (company.IsFailure)
                {
                    return false;
                }

                Title = company.Value.CompanyName;

                Input ??= new InputModel
                {
                    CompanyName = company.Value.CompanyName,
                    CompanyAddress = company.Value.CompanyAddress,
                    CompanyPostcode = company.Value.CompanyPostcode,
                    CompanyContact = company.Value.CompanyContact,
                    CompanyPhone = company.Value.CompanyPhone,
                    CompanyEmail = company.Value.CompanyEmail
                };

                return true;
            }

            return false;
        }
    }
}
