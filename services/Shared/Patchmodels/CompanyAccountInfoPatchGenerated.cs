using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class CompanyAccountInfoPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<string> ExternalAccountId { get; set; }
        public PatchOperation<string> ExternalCustomerId { get; set; }
    }
}
