using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Koasta.Service.Admin.Pages
{
    public class ProductTypeModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly ProductTypeRepository producttypes;

        public string Title { get; set; }
        public ProductType ProductType { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }

        public ProductTypeModel(UserManager<Employee> userManager,
                        RoleManager<EmployeeRole> roleManager,
                        ProductTypeRepository producttypes)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.producttypes = producttypes;
        }

        public async Task<IActionResult> OnGetAsync(int producttypeId)
        {
            if (await FetchData(producttypeId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<bool> FetchData(int producttypeId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem) {
                return false;
            }

            var result = (await producttypes.FetchProductType(producttypeId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Product Type found")
                                    .OnSuccess(e => e.Value);

            ProductType = result.IsSuccess ? result.Value : null;
            Title = ProductType.ProductTypeName;
            return result.IsSuccess;
        }
    }
}
