using System;
using System.Collections.Generic;
using Koasta.Shared.Models;

namespace Koasta.Service.OrderService.Models
{
    public class DtoFullOrder
    {
        public int OrderId { get; set; }
        public int? UserId { get; set; }
        public int OrderNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string VenueName { get; set; }
        public DateTime OrderTimeStamp { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string ExternalPaymentId { get; set; }
        public List<FullOrderItem> LineItems { get; set; }

        public static explicit operator DtoFullOrder(FullOrder order)
        {
            return new DtoFullOrder
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                OrderNumber = order.OrderNumber,
                FirstName = order.FirstName,
                LastName = order.LastName,
                VenueName = order.VenueName,
                OrderTimeStamp = order.OrderTimeStamp,
                OrderStatus = (OrderStatus)order.OrderStatus,
                ExternalPaymentId = order.ExternalPaymentId,
                LineItems = order.LineItems,
            };
        }
    }
}
