using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class ProductPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> ProductId { get; set; }
        public PatchOperation<int> ProductType { get; set; }
        public PatchOperation<string> ProductName { get; set; }
        public PatchOperation<string> ProductDescription { get; set; }
        public PatchOperation<decimal> Price { get; set; }
        public PatchOperation<string> Image { get; set; }
        public PatchOperation<bool> AgeRestricted { get; set; }
        public PatchOperation<int> ParentProductId { get; set; }
        public PatchOperation<int> VenueId { get; set; }
    }
}
