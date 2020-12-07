using Koasta.Shared.DbModels;
using Koasta.Shared.Models;

namespace Koasta.Service.OrderService.Models
{
    public class DtoOrderStatus
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public int OrderNumber { get; set; }

        public static explicit operator DtoOrderStatus(OrderStatusPair order)
        {
            return new DtoOrderStatus
            {
                OrderId = order.OrderId,
                Status = (OrderStatus)order.OrderStatus,
                OrderNumber = order.OrderNumber,
            };
        }

        public static explicit operator DtoOrderStatus(Order order)
        {
            return new DtoOrderStatus
            {
                OrderId = order.OrderId,
                Status = (OrderStatus)order.OrderStatus,
                OrderNumber = order.OrderNumber,
            };
        }
    }
}
