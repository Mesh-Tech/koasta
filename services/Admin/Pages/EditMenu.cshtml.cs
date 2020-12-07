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
    public class EditMenuModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly MenuRepository menus;
        private readonly VenueRepository venues;
        private readonly ProductRepository products;

        public string Title { get; set; } 
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        public List<Product> Products { get; set; }
        public int VenueId { get; set; }
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

        public EditMenuModel(UserManager<Employee> userManager,
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

        public async Task<IActionResult> OnGetAsync(int menuId)
        {
            if (await FetchData(menuId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int menuId)
        {
            if (!ModelState.IsValid)
            {
                await FetchData(menuId).ConfigureAwait(false);
                return this.TurboPage();
            }

            if (!await FetchData(menuId).ConfigureAwait(false))
            {
                return RedirectToPage("/Index");
            }

            var productIds = Input.ProductIds.Select(id => int.Parse(id)).ToList();
            var selectedProductIds = productIds
                .Where(id => ProductMap.ContainsKey(id))
                .Select(id => ProductMap[id])
                .Where(product => product.VenueId == VenueId)
                .Select(product => product.ProductId)
                .ToList();

            var result = await menus.ReplaceFullMenu(VenueId, new UpdatedMenu
            {
                MenuId = menuId,
                Products = selectedProductIds,
                MenuName = Input.MenuName,
                MenuDescription = Input.MenuDescription
            }).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return this.RedirectToPage("/Menu", new
                {
                    menuId
                });
            }

            return this.Page();
        }

        private async Task<bool> FetchData(int menuId) {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            var result = (await menus.FetchFullMenu(menuId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Menu found")
                                    .OnSuccess(e => e.Value);

            if (result.IsFailure)
            {
                return false;
            }

            VenueId = result.Value.VenueId;

            Products = await products.FetchVenueProducts(VenueId, 0, int.MaxValue)
                .Ensure(t => t.HasValue, "Venue products were found")
                .OnSuccess(t => t.Value)
                .OnBoth(t => t.IsSuccess ? t.Value : new List<Product>())
                .ConfigureAwait(false);

            ProductMap = new Dictionary<int, Product>();
            foreach (var product in Products)
            {
                ProductMap[product.ProductId] = product;
            }

            var venue = await venues.FetchVenue(VenueId)
                .Ensure(t => t.HasValue, "Venue was found")
                .OnSuccess(t => t.Value)
                .ConfigureAwait(false);

            if (!venue.IsSuccess)
            {
                return false;
            }

            if (!Role.CanAdministerSystem && (!Role.CanAdministerVenue || Employee.CompanyId != venue.Value.CompanyId))
            {
                return false;
            }

            Title = result.Value.MenuName;
            Input ??= new InputModel {
                MenuName = result.Value.MenuName,
                ProductIds = result.Value.Products.Select(p => p.ProductId.ToString()).ToList(),
                MenuDescription = result.Value.MenuDescription
            };

            return true;
        }
    }
}
