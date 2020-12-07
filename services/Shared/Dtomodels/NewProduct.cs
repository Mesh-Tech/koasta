using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Models
{
    public partial class NewProduct
    {
        [Column("venueId")]
        public int VenueId { get; set; }
    }
}
