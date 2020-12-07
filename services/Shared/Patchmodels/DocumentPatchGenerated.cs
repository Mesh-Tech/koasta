using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class DocumentPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> DocumentId { get; set; }
        public PatchOperation<int> CompanyId { get; set; }
        public PatchOperation<string> DocumentKey { get; set; }
        public PatchOperation<string> DocumentTitle { get; set; }
        public PatchOperation<string> DocumentDescription { get; set; }
    }
}
