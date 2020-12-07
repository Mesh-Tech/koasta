using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class Employee
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
        [Column("securityStamp")]
        public string SecurityStamp { get; set; }
        [Column("confirmed")]
        public bool Confirmed { get; set; }
    }
}
