using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class UpdatedReview
    {
        [Column("reviewSummary")]
        public string ReviewSummary { get; set; }
        [Column("reviewDetail")]
        public string ReviewDetail { get; set; }
        [Column("rating")]
        public int? Rating { get; set; }
        [Column("registeredInterest")]
        public bool RegisteredInterest { get; set; }
    }
}
