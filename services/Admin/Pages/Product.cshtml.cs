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
    public class ProductModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly ProductRepository products;
        private readonly VenueRepository venues;
        private readonly ProductTypeRepository productTypes;

        public string Title { get; set; }
        public Product Product { get; set; }
        public string ProductType { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = false)]
        public string Action { get; set; }

        public ProductModel(UserManager<Employee> userManager,
                        RoleManager<EmployeeRole> roleManager,
                        ProductTypeRepository productTypes,
                        VenueRepository venues,
                        ProductRepository products)
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
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int productId)
        {
            if (!string.Equals(Action, "delete", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Index");
            }

            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanAdministerVenue)
            {
                return RedirectToPage("/Index");
            }

            var getResult = (await products.FetchProduct(productId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Product found")
                                    .OnSuccess(e => e.Value);

            if (!getResult.IsSuccess)
            {
                return RedirectToPage("/Index");
            }

            var result = await products.DropProduct(productId).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return RedirectToPage("/Products", new
                {
                    venueId = getResult.Value.VenueId
                });
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<bool> FetchData(int productId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanWorkWithVenue)
            {
                return false;
            }

            var result = (await products.FetchProduct(productId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Product found")
                                    .OnSuccess(e => e.Value);

            Product = result.IsSuccess ? result.Value : null;

            var productVenue = (await venues.FetchVenue(Product.VenueId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Venue found")
                                    .OnSuccess(e => e.Value);

            var result2 = (await productTypes.FetchProductType(Product.ProductType).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Product type found")
                                    .OnSuccess(e => e.Value);

            ProductType = result2.IsSuccess ? result2.Value.ProductTypeName : null;

            if (productVenue.IsFailure)
            {
                return false;
            }

            if (!Role.CanAdministerSystem && productVenue.Value.CompanyId != Employee.CompanyId)
            {
                return false;
            }

            ViewData["SubnavVenueId"] = Product.VenueId;
            Title = Product.ProductName;
            return result.IsSuccess;
        }
    }
}
