using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class FullOrderItem
    {
        [Column("orderLineId")]
        public int Id { get; set; }
        [Column("productName")]
        public string ProductName { get; set; }
        [Column("quantity")]
        public int Quantity { get; set; }
        [Column("amount")]
        public decimal Amount { get; set; }
    }
}
