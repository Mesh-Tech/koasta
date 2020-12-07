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
    public class AddProductTypeModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly ProductTypeRepository companies;

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
            public string ProductTypeName { get; set; }
        }

        public AddProductTypeModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              ProductTypeRepository companies)
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

            return (await companies.CreateProductType(new ProductType
            {
                ProductTypeName = Input.ProductTypeName
            })
            .Ensure(c => c.HasValue, "Product Type was created")
            .OnSuccess(c => this.RedirectToPage("/ProductType", new {
                producttypeId = c.Value
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
