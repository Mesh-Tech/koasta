using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class FullEmployee
    {
        [Column("employeeId")]
        public int EmployeeId { get; set; }
        [Column("employeeName")]
        public string EmployeeName { get; set; }
        [Column("username")]
        public string Username { get; set; }
        [Column("passwordHash")]
        public string PasswordHash { get; set; }
        [Column("companyId")]
        public int CompanyId { get; set; }
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("roleId")]
        public int RoleId { get; set; }
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
