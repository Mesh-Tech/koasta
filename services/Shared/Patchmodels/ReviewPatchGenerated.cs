using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class ReviewPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> ReviewId { get; set; }
        public PatchOperation<int> VenueId { get; set; }
        public PatchOperation<int> UserId { get; set; }
        public PatchOperation<string> ReviewSummary { get; set; }
        public PatchOperation<string> ReviewDetail { get; set; }
        public PatchOperation<int> Rating { get; set; }
        public PatchOperation<bool> RegisteredInterest { get; set; }
        public PatchOperation<bool> Approved { get; set; }
        public PatchOperation<DateTime> Created { get; set; }
        public PatchOperation<DateTime> Updated { get; set; }
    }
}
