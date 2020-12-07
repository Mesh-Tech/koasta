using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class VenueDocument
    {
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("documentId")]
        public int DocumentId { get; set; }
    }
}
