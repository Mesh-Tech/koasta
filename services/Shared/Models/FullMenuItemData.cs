using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Models
{
    public class FullMenuItemData
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
        [Column("productId")]
        public int ProductId { get; set; }
        [Column("productTypeName")]
        public string ProductTypeName { get; set; }
        [Column("productName")]
        public string ProductName { get; set; }
        [Column("productDescription")]
        public string ProductDescription { get; set; }
        [Column("price")]
        public decimal Price { get; set; }
        [Column("image")]
        public string Image { get; set; }
        [Column("ageRestricted")]
        public bool AgeRestricted { get; set; }
    }
}
