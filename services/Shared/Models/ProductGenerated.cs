using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class Product
    {
        [Column("productId")]
        public int ProductId { get; set; }
        [Column("productTypeId")]
        public int ProductType { get; set; }
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
        [Column("parentProductId")]
        public int? ParentProductId { get; set; }
        [Column("venueId")]
        public int VenueId { get; set; }
    }
}
