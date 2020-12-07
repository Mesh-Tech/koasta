using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class Order
    {
        [Column("orderId")]
        public int OrderId { get; set; }
        [Column("orderNumber")]
        public int OrderNumber { get; set; }
        [Column("userId")]
        public int UserId { get; set; }
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("orderStatus")]
        public int OrderStatus { get; set; }
        [Column("employeeId")]
        public int? EmployeeId { get; set; }
        [Column("orderTimeStamp")]
        public DateTime OrderTimeStamp { get; set; }
        [Column("externalPaymentId")]
        public string ExternalPaymentId { get; set; }
        [Column("lineItems")]
        public List<OrderLine> LineItems { get; set; }
        [Column("total")]
        public decimal Total { get; set; }
        [Column("serviceCharge")]
        public decimal ServiceCharge { get; set; }
        [Column("nonce")]
        public string Nonce { get; set; }
        [Column("orderNotes")]
        public string OrderNotes { get; set; }
        [Column("servingType")]
        public int ServingType { get; set; }
        [Column("tableName")]
        public string Table { get; set; }
    }
}
