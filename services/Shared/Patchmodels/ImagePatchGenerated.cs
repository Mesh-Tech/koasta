using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class ImagePatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> ImageId { get; set; }
        public PatchOperation<int> CompanyId { get; set; }
        public PatchOperation<string> ImageKey { get; set; }
        public PatchOperation<string> ImageTitle { get; set; }
    }
}
