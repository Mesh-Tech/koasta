using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Models
{
    public partial class UpdatedProduct
    {
        [Column("venueId")]
        public int VenueId { get; set; }
    }
}
