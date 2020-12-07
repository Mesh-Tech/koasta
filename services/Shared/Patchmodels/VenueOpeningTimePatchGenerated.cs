using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class VenueOpeningTimePatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> VenueOpeningTimeId { get; set; }
        public PatchOperation<int> VenueId { get; set; }
        public PatchOperation<TimeSpan> StartTime { get; set; }
        public PatchOperation<TimeSpan> EndTime { get; set; }
        public PatchOperation<int> DayOfWeek { get; set; }
    }
}
