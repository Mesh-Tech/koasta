using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class MenuAvailabilityPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> MenuAvailabilityId { get; set; }
        public PatchOperation<int> MenuId { get; set; }
        public PatchOperation<DateTime> TimeStart { get; set; }
        public PatchOperation<DateTime> TimeEnd { get; set; }
        public PatchOperation<int> Day { get; set; }
    }
}
