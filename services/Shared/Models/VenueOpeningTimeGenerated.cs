using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class VenueOpeningTime
    {
        [Column("venueOpeningTimeId")]
        public int VenueOpeningTimeId { get; set; }
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("startTime")]
        public TimeSpan? StartTime { get; set; }
        [Column("endTime")]
        public TimeSpan? EndTime { get; set; }
        [Column("dayOfWeek")]
        public int DayOfWeek { get; set; }
    }
}
