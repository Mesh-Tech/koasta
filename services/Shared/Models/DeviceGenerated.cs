using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class Device
    {
        [Column("deviceId")]
        public int DeviceId { get; set; }
        [Column("userId")]
        public int UserId { get; set; }
        [Column("deviceToken")]
        public string Token { get; set; }
        [Column("platform")]
        public int Platform { get; set; }
        [Column("updated")]
        public DateTime UpdateTimestamp { get; set; }
    }
}
