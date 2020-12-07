using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class EmployeeSessionPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> SessionId { get; set; }
        public PatchOperation<int> EmployeeId { get; set; }
        public PatchOperation<string> AuthToken { get; set; }
        public PatchOperation<string> RefreshToken { get; set; }
        public PatchOperation<DateTime> Expiry { get; set; }
        public PatchOperation<DateTime> RefreshExpiry { get; set; }
    }
}
