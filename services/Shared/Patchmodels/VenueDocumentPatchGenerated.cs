using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class VenueDocumentPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> VenueId { get; set; }
        public PatchOperation<int> DocumentId { get; set; }
    }
}
