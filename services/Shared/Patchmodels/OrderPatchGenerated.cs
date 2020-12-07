using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class OrderPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> OrderId { get; set; }
        public PatchOperation<int> OrderNumber { get; set; }
        public PatchOperation<int> UserId { get; set; }
        public PatchOperation<int> VenueId { get; set; }
        public PatchOperation<int> OrderStatus { get; set; }
        public PatchOperation<int> EmployeeId { get; set; }
        public PatchOperation<DateTime> OrderTimeStamp { get; set; }
        public PatchOperation<string> ExternalPaymentId { get; set; }
        public PatchOperation<decimal> Total { get; set; }
        public PatchOperation<decimal> ServiceCharge { get; set; }
        public PatchOperation<string> Nonce { get; set; }
        public PatchOperation<string> OrderNotes { get; set; }
        public PatchOperation<int> ServingType { get; set; }
        public PatchOperation<string> Table { get; set; }
    }
}
