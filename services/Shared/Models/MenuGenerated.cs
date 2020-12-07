using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class Menu
    {
        [Column("menuId")]
        public int MenuId { get; set; }
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("menuDescription")]
        public string MenuDescription { get; set; }
        [Column("menuName")]
        public string MenuName { get; set; }
        [Column("menuImage")]
        public string MenuImage { get; set; }
    }
}
