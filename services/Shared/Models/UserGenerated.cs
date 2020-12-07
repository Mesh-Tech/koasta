using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class User
    {
        [Column("userId")]
        public int UserId { get; set; }
        [Column("registrationId")]
        public string RegistrationId { get; set; }
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
        [Column("externalPaymentProcessorId")]
        public string ExternalPaymentProcessorId { get; set; }
        [Column("appleUserIdentifier")]
        public string AppleUserIdentifier { get; set; }
        [Column("facebookUserIdentifier")]
        public string FacebookUserIdentifier { get; set; }
        [Column("googleUserIdentifier")]
        public string GoogleUserIdentifier { get; set; }
    }
}
