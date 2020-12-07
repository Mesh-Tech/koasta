using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class DevicePatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> DeviceId { get; set; }
        public PatchOperation<int> UserId { get; set; }
        public PatchOperation<string> Token { get; set; }
        public PatchOperation<int> Platform { get; set; }
        public PatchOperation<DateTime> UpdateTimestamp { get; set; }
    }
}
