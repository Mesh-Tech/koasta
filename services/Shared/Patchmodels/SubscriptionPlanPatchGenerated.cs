using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class SubscriptionPlanPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> PlanId { get; set; }
        public PatchOperation<int> CompanyId { get; set; }
        public PatchOperation<string> ExternalPlanId { get; set; }
    }
}
