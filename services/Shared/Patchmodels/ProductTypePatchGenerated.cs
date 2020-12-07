using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class ProductTypePatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> ProductTypeId { get; set; }
        public PatchOperation<string> ProductTypeName { get; set; }
    }
}
