using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class EmployeeRole
    {
        [Column("roleId")]
        public int RoleId { get; set; }
        [Column("roleName")]
        public string RoleName { get; set; }
        [Column("canWorkWithVenue")]
        public bool CanWorkWithVenue { get; set; }
        [Column("canAdministerVenue")]
        public bool CanAdministerVenue { get; set; }
        [Column("canWorkWithCompany")]
        public bool CanWorkWithCompany { get; set; }
        [Column("canAdministerCompany")]
        public bool CanAdministerCompany { get; set; }
        [Column("canAdministerSystem")]
        public bool CanAdministerSystem { get; set; }
    }
}
