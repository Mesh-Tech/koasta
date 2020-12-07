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
    public class EditProductTypeModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly ProductTypeRepository producttypes;

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
            public string ProductTypeName { get; set; }
        }

        public EditProductTypeModel(UserManager<Employee> userManager,
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
                return RedirectToPage("/ProductTypes");
            }
        }

        public async Task<IActionResult> OnPostAsync(int producttypeId)
        {
            if (!ModelState.IsValid)
            {
                await FetchData(producttypeId).ConfigureAwait(false);
                return this.TurboPage();
            }

            return (await producttypes.UpdateProductType(new ProductTypePatch
            {
                ResourceId = producttypeId,
                ProductTypeName = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.ProductTypeName },
            })
            .OnSuccess(() => this.RedirectToPage("/ProductType", new {
                producttypeId
            }))
            .OnFailure(() => this.Page())
            .ConfigureAwait(false)).Value;
        }

        private async Task<bool> FetchData(int producttypeId) {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerSystem)
            {
                return false;
            }

            var producttype = (await producttypes.FetchProductType(producttypeId).ConfigureAwait(false))
                        .Ensure(e => e.HasValue, "Product Type found")
                        .OnSuccess(e => e.Value);

            if (producttype.IsFailure)
            {
                return false;
            }

            Title = producttype.Value.ProductTypeName;

            Input ??= new InputModel
            {
                ProductTypeName = producttype.Value.ProductTypeName,
            };

            return true;
        }
    }
}
