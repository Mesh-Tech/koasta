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
using System;

namespace Koasta.Service.Admin.Pages
{
    public class EditProductModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly ProductRepository products;
        private readonly VenueRepository venues;
        private readonly ProductTypeRepository productTypes;

        public string Title { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        public List<ProductType> ProductTypes { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [MaxLength(50)]
            [Display(Name = "Name")]
            public string ProductName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Type")]
            public string ProductTypeId { get; set; }

            [DataType(DataType.Text)]
            [MaxLength(8000)]
            [Display(Name = "Description")]
            public string ProductDescription { get; set; }

            [Required]
            [DataType(DataType.Currency)]
            [Display(Name = "Price")]
            public decimal Price { get; set; }

            [Required]
            [Range(typeof(bool), "false", "true", ErrorMessage = "Age Restricted is required.")]
            [Display(Name = "Age Restricted")]
            public bool AgeRestricted { get; set; }
        }

        public EditProductModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              VenueRepository venues,
                              ProductRepository products,
                              ProductTypeRepository productTypes)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.products = products;
            this.venues = venues;
            this.productTypes = productTypes;
        }

        public async Task<IActionResult> OnGetAsync(int productId)
        {
            if (await FetchData(productId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Products");
            }
        }

        public async Task<IActionResult> OnPostAsync(int productId)
        {
            if (!ModelState.IsValid)
            {
                await FetchData(productId).ConfigureAwait(false);
                return this.TurboPage();
            }

            if (!await FetchData(productId).ConfigureAwait(false)) {
                return this.TurboPage();
            }

            var selectedTypeId = int.Parse(Input.ProductTypeId);
            var selectedType = ProductTypes.Find(r => r.ProductTypeId == selectedTypeId);

            if (selectedType == null)
            {
                return this.TurboPage();
            }

            return (await products.UpdateProduct(new ProductPatch
            {
                ResourceId = productId,
                ProductType = new PatchOperation<int> { Operation = OperationKind.Update, Value = selectedTypeId },
                ProductName = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.ProductName },
                ProductDescription = new PatchOperation<string> { Operation = OperationKind.Update, Value = Input.ProductDescription },
                Price = new PatchOperation<decimal> { Operation = OperationKind.Update, Value = Input.Price },
                AgeRestricted = new PatchOperation<bool> { Operation = OperationKind.Update, Value = Input.AgeRestricted }
            })
            .OnSuccess(() => this.RedirectToPage("/Product", new {
                productId
            }))
            .OnFailure(() => this.Page())
            .ConfigureAwait(false)).Value;
        }

        private async Task<bool> FetchData(int productId) {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerVenue)
            {
                return false;
            }

            ProductTypes = await productTypes.FetchProductTypes(0, int.MaxValue)
                .Ensure(t => t.HasValue, "Product Types were found")
                .OnSuccess(t => t.Value)
                .OnBoth(t => t.IsSuccess ? t.Value : new List<ProductType>())
                .ConfigureAwait(false);

            var product = (await products.FetchProduct(productId).ConfigureAwait(false))
                        .Ensure(e => e.HasValue, "Product found")
                        .OnSuccess(e => e.Value);

            if (product.IsFailure)
            {
                return false;
            }

            var venue = await venues.FetchVenue(product.Value.VenueId)
                .Ensure(t => t.HasValue, "Venue was found")
                .OnSuccess(t => t.Value)
                .ConfigureAwait(false);

            if (!venue.IsSuccess)
            {
                return false;
            }

            if (!Role.CanAdministerSystem && Employee.CompanyId != venue.Value.CompanyId)
            {
                return false;
            }

            Title = product.Value.ProductName;

            Input ??= new InputModel
            {
                ProductName = product.Value.ProductName,
                ProductTypeId = product.Value.ProductType.ToString(),
                ProductDescription = product.Value.ProductDescription,
                Price = product.Value.Price,
                AgeRestricted = product.Value.AgeRestricted,
            };

            return true;
        }
    }
}
