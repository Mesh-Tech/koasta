using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class NewOrder
    {
        [Column("orderNumber")]
        public int OrderNumber { get; set; }
        [Column("userId")]
        public int UserId { get; set; }
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("lineItems")]
        public List<NewOrderItem> LineItems { get; set; }
        [Column("total")]
        public decimal Total { get; set; }
        [Column("serviceCharge")]
        public decimal ServiceCharge { get; set; }
    }
}
