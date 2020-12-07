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
using System.Linq;

namespace Koasta.Service.Admin.Pages
{
    public class AddMenuModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly MenuRepository menus;
        private readonly VenueRepository venues;
        private readonly ProductRepository products;

        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        public List<Product> Products { get; set; }
        private Dictionary<int, Product> ProductMap { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [MaxLength(255)]
            [Display(Name = "Name")]
            public string MenuName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Products")]
            public List<string> ProductIds { get; set; }

            [DataType(DataType.Text)]
            [MaxLength(8000)]
            [Display(Name = "Description")]
            public string MenuDescription { get; set; }
        }

        public AddMenuModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              VenueRepository venues,
                              MenuRepository menus,
                              ProductRepository products)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.menus = menus;
            this.venues = venues;
            this.products = products;
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

            var productIds = Input.ProductIds.Select(id => int.Parse(id)).ToList();
            var selectedProductIds = productIds
                .Where(id => ProductMap.ContainsKey(id))
                .Select(id => ProductMap[id])
                .Where(product => product.VenueId == venueId)
                .Select(product => product.ProductId)
                .ToList();

            var result = await menus.CreateFullMenu(new NewMenu
            {
                Products = selectedProductIds,
                MenuName = Input.MenuName,
                MenuDescription = Input.MenuDescription,
                VenueId = venueId
            }).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return this.RedirectToPage("/Menu", new
                {
                    menuId = result.Value
                });
            }

            return this.Page();
        }

        private async Task<bool> FetchData(int venueId) {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            Products = await products.FetchVenueProducts(venueId, 0, int.MaxValue)
                .Ensure(t => t.HasValue, "Venue products were found")
                .OnSuccess(t => t.Value)
                .OnBoth(t => t.IsSuccess ? t.Value : new List<Product>())
                .ConfigureAwait(false);

            ProductMap = new Dictionary<int, Product>();
            foreach(var product in Products)
            {
                ProductMap[product.ProductId] = product;
            }

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
