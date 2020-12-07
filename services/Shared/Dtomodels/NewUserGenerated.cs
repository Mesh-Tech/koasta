using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class NewUser
    {
        [Column("firstName")]
        public string FirstName { get; set; }
        [Column("lastName")]
        public string LastName { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("dob")]
        public string Dob { get; set; }
        [Column("isVerified")]
        public bool IsVerified { get; set; }
        [Column("wantAdvertising")]
        public bool WantAdvertising { get; set; }
        [Column("registrationId")]
        public string RegistrationId { get; set; }
    }
}
