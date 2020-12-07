using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class UserPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> UserId { get; set; }
        public PatchOperation<string> RegistrationId { get; set; }
        public PatchOperation<string> FirstName { get; set; }
        public PatchOperation<string> LastName { get; set; }
        public PatchOperation<string> Email { get; set; }
        public PatchOperation<string> Dob { get; set; }
        public PatchOperation<bool> IsVerified { get; set; }
        public PatchOperation<bool> WantAdvertising { get; set; }
        public PatchOperation<string> ExternalPaymentProcessorId { get; set; }
        public PatchOperation<string> AppleUserIdentifier { get; set; }
        public PatchOperation<string> FacebookUserIdentifier { get; set; }
        public PatchOperation<string> GoogleUserIdentifier { get; set; }
    }
}
