using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class FeatureFlagPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> FlagId { get; set; }
        public PatchOperation<string> Name { get; set; }
        public PatchOperation<string> Description { get; set; }
        public PatchOperation<bool> Value { get; set; }
    }
}
