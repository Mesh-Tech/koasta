using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class NewEmployeeSession
    {
        [Column("employeeId")]
        public int EmployeeId { get; set; }
        [Column("authToken")]
        public string AuthToken { get; set; }
        [Column("refreshToken")]
        public string RefreshToken { get; set; }
        [Column("expiry")]
        public DateTime Expiry { get; set; }
        [Column("refreshExpiry")]
        public DateTime RefreshExpiry { get; set; }
    }
}
