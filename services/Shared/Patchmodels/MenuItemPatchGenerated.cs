using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class MenuItemPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> MenuItemId { get; set; }
        public PatchOperation<int> VenueId { get; set; }
        public PatchOperation<int> MenuId { get; set; }
        public PatchOperation<string> ProductId { get; set; }
    }
}
