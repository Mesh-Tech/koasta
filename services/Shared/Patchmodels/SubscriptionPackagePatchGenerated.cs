using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class SubscriptionPackagePatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> PackageId { get; set; }
        public PatchOperation<string> PackageName { get; set; }
        public PatchOperation<string> ExternalPackageId { get; set; }
    }
}
