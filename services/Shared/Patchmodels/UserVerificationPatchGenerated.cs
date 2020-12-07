using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class UserVerificationPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> VerificationId { get; set; }
        public PatchOperation<string> PhoneNumber { get; set; }
        public PatchOperation<int> UserId { get; set; }
        public PatchOperation<string> VerificationToken { get; set; }
        public PatchOperation<DateTime> Expiry { get; set; }
    }
}
