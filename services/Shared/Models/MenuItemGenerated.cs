using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class MenuItem
    {
        [Column("menuItemId")]
        public int MenuItemId { get; set; }
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("menuId")]
        public int MenuId { get; set; }
        [Column("productId")]
        public string ProductId { get; set; }
    }
}
