using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace Koasta.Service.Admin.Pages
{
    public class OrderModel : PageModel
    {
        private readonly UserManager<Employee> userManager;
        private readonly RoleManager<EmployeeRole> roleManager;
        private readonly OrderRepository orders;

        public string Title { get; set; }
        public FullOrder Order { get; set; }
        public Employee Employee { get; set; }
        public EmployeeRole Role { get; set; }

        public OrderModel(UserManager<Employee> userManager,
                        RoleManager<EmployeeRole> roleManager,
                        OrderRepository orders)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.orders = orders;
        }

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            if (await FetchData(orderId).ConfigureAwait(false))
            {
                return Page();
            }
            else
            {
                return RedirectToPage("/Index");
            }
        }

        private async Task<bool> FetchData(int orderId)
        {
            Employee = await userManager.GetUserAsync(User).ConfigureAwait(false);
            Role = await roleManager.FindByIdAsync(Employee.RoleId.ToString()).ConfigureAwait(false);

            if (!Role.CanWorkWithVenue) {
                return false;
            }

            var result = (await orders.FetchFullOrder(orderId).ConfigureAwait(false))
                                    .Ensure(e => e.HasValue, "Order found")
                                    .OnSuccess(e => e.Value);

            Order = result.IsSuccess ? result.Value : null;
            if (!Role.CanAdministerSystem && (Order.CompanyId != Employee.CompanyId)) {
                return false;
            }

            Title = $"Order {Order.OrderNumber}";
            return result.IsSuccess;
        }

        public string GetOrderStatus()
        {
            return Order.OrderStatus switch
            {
                0 => "Unknown",
                1 => "Ordered",
                2 => "In Progress",
                3 => "Ready",
                4 => "Complete",
                5 => "Rejected",
                _ => "Unknown",
            };
        }
    }
}
