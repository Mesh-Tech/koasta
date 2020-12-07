using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class UserSessionPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> SessionId { get; set; }
        public PatchOperation<int> UserId { get; set; }
        public PatchOperation<string> AuthToken { get; set; }
        public PatchOperation<int> TokenType { get; set; }
        public PatchOperation<DateTime> AuthTokenExpiry { get; set; }
    }
}
