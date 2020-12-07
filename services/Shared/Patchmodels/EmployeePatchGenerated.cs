using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.PatchModels
{
    [CompilerGeneratedAttribute()]
    public partial class EmployeePatch
    {
        [JsonIgnore]
        public int ResourceId { get; set; }

        public PatchOperation<int> EmployeeId { get; set; }
        public PatchOperation<string> EmployeeName { get; set; }
        public PatchOperation<string> Username { get; set; }
        public PatchOperation<string> PasswordHash { get; set; }
        public PatchOperation<int> CompanyId { get; set; }
        public PatchOperation<int> VenueId { get; set; }
        public PatchOperation<int> RoleId { get; set; }
        public PatchOperation<string> SecurityStamp { get; set; }
        public PatchOperation<bool> Confirmed { get; set; }
    }
}
