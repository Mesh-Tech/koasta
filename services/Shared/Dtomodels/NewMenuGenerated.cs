using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class NewMenu
    {
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("menuName")]
        public string MenuName { get; set; }
        [Column("menuDescription")]
        public string MenuDescription { get; set; }
        [Column("menuImage")]
        public string MenuImage { get; set; }
        [Column("products")]
        public List<int> Products { get; set; }
    }
}
