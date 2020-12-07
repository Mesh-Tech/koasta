using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class MenuAvailability
    {
        [Column("menuAvailabilityId")]
        public int MenuAvailabilityId { get; set; }
        [Column("menuId")]
        public int MenuId { get; set; }
        [Column("timeStart")]
        public DateTime TimeStart { get; set; }
        [Column("timeEnd")]
        public DateTime TimeEnd { get; set; }
        [Column("day")]
        public int Day { get; set; }
    }
}
