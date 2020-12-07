using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Models
{
    public class AggregatedVenueVote
    {
        [Column("total")]
        public int Total { get; set; }

        [Column("venueId")]
        public int VenueId { get; set; }

        [Column("venueName")]
        public string VenueName { get; set; }
    }
}
