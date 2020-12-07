using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.DbModels
{
    public class ProductPricePair
    {
        [Column("productId")]
        public int ProductId { get; set; }
        [Column("price")]
        public decimal Price { get; set; }
    }
}
