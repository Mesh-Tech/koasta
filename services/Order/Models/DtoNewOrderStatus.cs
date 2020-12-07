using Koasta.Shared.Models;

namespace Koasta.Service.OrderService.Models
{
    public class DtoNewOrderStatus
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public int OrderNumber { get; set; }
        public decimal Total { get; set; }
        public decimal ServiceCharge { get; set; }

        public static explicit operator DtoNewOrderStatus(Order order)
        {
            return new DtoNewOrderStatus
            {
                OrderId = order.OrderId,
                Status = (OrderStatus)order.OrderStatus,
                OrderNumber = order.OrderNumber,
                Total = order.Total,
                ServiceCharge = order.ServiceCharge
            };
        }
    }
}
