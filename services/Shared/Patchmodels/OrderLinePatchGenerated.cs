using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class OrderLinePatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> OrderLineId { get; set; }
        public PatchOperation<int> OrderId { get; set; }
        public PatchOperation<int> ProductId { get; set; }
        public PatchOperation<int> Quantity { get; set; }
        public PatchOperation<decimal> Amount { get; set; }
    }
}
