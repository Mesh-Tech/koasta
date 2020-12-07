using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using System.ComponentModel.DataAnnotations;
using Koasta.Service.Admin.Utils;
using System.Collections.Generic;

namespace Koasta.Service.Admin.Pages
{
    public class AddProductModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly ProductRepository products;
        private readonly VenueRepository venues;
        private readonly ProductTypeRepository productTypes;

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
            [Range(typeof(bool), "false", "true", ErrorMessage="Age Restricted is required.")]
            [Display(Name = "Age Restricted")]
            public bool AgeRestricted { get; set; }
        }

        public AddProductModel(UserManager<Employee> userManager,
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

        public async Task<IActionResult> OnGetAsync(int venueId)
        {
            if (await FetchData(venueId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int venueId)
        {
            if (!ModelState.IsValid)
            {
                await FetchData(venueId).ConfigureAwait(false);
                return this.TurboPage();
            }

            if (!await FetchData(venueId).ConfigureAwait(false))
            {
                return RedirectToPage("/Index");
            }

            var selectedTypeId = int.Parse(Input.ProductTypeId);
            var selectedType = ProductTypes.Find(r => r.ProductTypeId == selectedTypeId);

            if (selectedType == null)
            {
                return this.TurboPage();
            }

            return (await products.CreateProduct(new Product
            {
                ProductType = selectedTypeId,
                ProductName = Input.ProductName,
                ProductDescription = Input.ProductDescription,
                Price = Input.Price,
                AgeRestricted = Input.AgeRestricted,
                VenueId = venueId,
            })
            .Ensure(c => c.HasValue, "Product was created")
            .OnSuccess(c => this.RedirectToPage("/Product", new {
                productId = c.Value
            }))
            .OnFailure(() => this.Page())
            .ConfigureAwait(false)).Value;
        }

        private async Task<bool> FetchData(int venueId) {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            ProductTypes = await productTypes.FetchProductTypes(0, int.MaxValue)
                .Ensure(t => t.HasValue, "Product Types were found")
                .OnSuccess(t => t.Value)
                .OnBoth(t => t.IsSuccess ? t.Value : new List<ProductType>())
                .ConfigureAwait(false);

            if (Role.CanAdministerSystem) {
                return true;
            }

            var venue = await venues.FetchVenue(venueId)
                .Ensure(t => t.HasValue, "Venue was found")
                .OnSuccess(t => t.Value)
                .ConfigureAwait(false);

            if (!venue.IsSuccess) {
                return false;
            }

            if(!Role.CanAdministerVenue || Employee.CompanyId != venue.Value.CompanyId)
            {
                return false;
            }

            return true;
        }
    }
}
