using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class UserNumberPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> NumberId { get; set; }
        public PatchOperation<int> UserId { get; set; }
        public PatchOperation<string> PhoneNumber { get; set; }
    }
}
