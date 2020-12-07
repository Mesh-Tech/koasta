using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class Review
    {
        [Column("reviewId")]
        public int ReviewId { get; set; }
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
        [Column("created")]
        public DateTime Created { get; set; }
        [Column("updated")]
        public DateTime Updated { get; set; }
    }
}
