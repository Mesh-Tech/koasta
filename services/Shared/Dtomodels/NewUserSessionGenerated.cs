using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class NewUserSession
    {
        [Column("userId")]
        public int UserId { get; set; }
        [Column("authToken")]
        public string AuthToken { get; set; }
        [Column("refreshToken")]
        public string RefreshToken { get; set; }
        [Column("authTokenExpiry")]
        public DateTime AuthTokenExpiry { get; set; }
        [Column("refreshTokenExpiry")]
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
