using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Models
{
    public class VenueItem
    {
        [Column("companyId")]
        public int CompanyId { get; set; }
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("venueName")]
        public string VenueName { get; set; }
    }
}
