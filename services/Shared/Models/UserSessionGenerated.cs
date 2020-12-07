using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class UserSession
    {
        [Column("sessionId")]
        public int SessionId { get; set; }
        [Column("userId")]
        public int UserId { get; set; }
        [Column("authToken")]
        public string AuthToken { get; set; }
        [Column("type")]
        public int TokenType { get; set; }
        [Column("authTokenExpiry")]
        public DateTime AuthTokenExpiry { get; set; }
    }
}
