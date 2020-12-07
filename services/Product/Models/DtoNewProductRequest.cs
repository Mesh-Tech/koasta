using System.ComponentModel.DataAnnotations;

namespace Koasta.Service.ProductService.Models
{
    public class DtoNewProductRequest
    {
        [Required]
        public int ProductTypeId { get; set; }
        [Required]
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        [Required]
        public decimal Price { get; set; }
        public string Image { get; set; }
        [Required]
        public bool AgeRestricted { get; set; }
        public int? ParentProductId { get; set; }
    }
}
