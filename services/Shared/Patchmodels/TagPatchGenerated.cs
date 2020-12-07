using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class TagPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> TagId { get; set; }
        public PatchOperation<string> TagName { get; set; }
    }
}
