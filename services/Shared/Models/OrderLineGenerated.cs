using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class OrderLine
    {
        [Column("orderLineId")]
        public int OrderLineId { get; set; }
        [Column("orderId")]
        public int OrderId { get; set; }
        [Column("productId")]
        public int ProductId { get; set; }
        [Column("quantity")]
        public int Quantity { get; set; }
        [Column("amount")]
        public decimal Amount { get; set; }
    }
}
