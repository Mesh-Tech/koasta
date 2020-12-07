using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Models
{
    public partial class FullOrder
    {
        [Column("companyId")]
        public int CompanyId { get; set; }

        [Column("venueId")]
        public int VenueId { get; set; }
    }
}
