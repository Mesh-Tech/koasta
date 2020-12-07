using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Koasta.Shared.Types;

namespace Koasta.Service.Admin.Pages
{
    public class ProductsModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly ProductRepository products;

        public string Title { get; set; }
        public List<Product> Products { get; set; }
        public int TotalResults { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; }
        public bool HasNextPage { get; set; }
        public int VenueId { get; set; }

        public ProductsModel(UserManager<Employee> userManager,
                              RoleManager<EmployeeRole> roleManager,
                              ProductRepository products)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.products = products;
        }

        public async Task<IActionResult> OnGetAsync(int venueId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);
            ViewData["SubnavVenueId"] = venueId;

            if (!Role.CanWorkWithVenue && Employee.VenueId != venueId)
            {
                return RedirectToPage("/Index");
            }

            var results = (await products.FetchCountedVenueProducts(venueId, PageNumber, 20).ConfigureAwait(false))
                    .Ensure(e => e.HasValue, "Products found")
                    .OnSuccess(e => e.Value)
                    .OnBoth(e => e.IsSuccess ? e.Value : new PaginatedResult<Product> { Data = new List<Product>(), Count = 0 });
            TotalResults = results.Count;
            Products = results.Data;
            Title = $"Products ({TotalResults})";
            HasNextPage = (PageNumber + 1) <= (TotalResults / 20);
            VenueId = venueId;

            return Page();
        }
    }
}
