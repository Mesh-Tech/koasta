using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class CompanyPatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> CompanyId { get; set; }
        public PatchOperation<string> CompanyName { get; set; }
        public PatchOperation<string> CompanyAddress { get; set; }
        public PatchOperation<string> CompanyPostcode { get; set; }
        public PatchOperation<string> CompanyContact { get; set; }
        public PatchOperation<string> CompanyPhone { get; set; }
        public PatchOperation<string> CompanyEmail { get; set; }
        public PatchOperation<string> ExternalAccountId { get; set; }
        public PatchOperation<string> ExternalCustomerId { get; set; }
        public PatchOperation<string> ExternalAccessToken { get; set; }
        public PatchOperation<string> ExternalRefreshToken { get; set; }
        public PatchOperation<DateTime> ExternalTokenExpiry { get; set; }
        public PatchOperation<string> ReferenceCode { get; set; }
    }
}
