using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class EmployeeRolePatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> RoleId { get; set; }
        public PatchOperation<string> RoleName { get; set; }
        public PatchOperation<bool> CanWorkWithVenue { get; set; }
        public PatchOperation<bool> CanAdministerVenue { get; set; }
        public PatchOperation<bool> CanWorkWithCompany { get; set; }
        public PatchOperation<bool> CanAdministerCompany { get; set; }
        public PatchOperation<bool> CanAdministerSystem { get; set; }
    }
}
