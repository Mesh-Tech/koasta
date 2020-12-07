using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Models
{
    public class CompanyOrderStatus
    {
        [Column("venueId")]
        public int VenueId { get; set; }

        [Column("incomplete")]
        public int Incomplete { get; set; }

        [Column("complete")]
        public int Complete { get; set; }

        [Column("failed")]
        public int Failed { get; set; }

        [Column("total")]
        public int Total { get; set; }
    }
}
