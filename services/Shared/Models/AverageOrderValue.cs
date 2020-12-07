using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Models
{
    public class AverageOrderValue
    {
        [Column("venueid")]
        public int VenueId { get; set; }

        [Column("orders")]
        public int? Count { get; set; }

        [Column("total")]
        public decimal Sum { get; set; }

        [Column("average")]
        public decimal Average { get; set; }
    }
}
