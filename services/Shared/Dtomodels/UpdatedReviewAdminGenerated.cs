using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class UpdatedReviewAdmin
    {
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("userId")]
        public int? UserId { get; set; }
        [Column("reviewSummary")]
        public string ReviewSummary { get; set; }
        [Column("reviewDetail")]
        public string ReviewDetail { get; set; }
        [Column("rating")]
        public int? Rating { get; set; }
        [Column("registeredInterest")]
        public bool RegisteredInterest { get; set; }
        [Column("approved")]
        public bool Approved { get; set; }
    }
}
