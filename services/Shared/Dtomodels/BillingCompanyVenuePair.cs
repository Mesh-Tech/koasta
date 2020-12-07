using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Models
{
    public class BillingCompanyVenuePair
    {
        [Column("companyId")]
        public int CompanyId { get; set; }
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("venueName")]
        public string VenueName { get; set; }
        [Column("externalAccountId")]
        public string ExternalAccountId { get; set; }
    }
}
