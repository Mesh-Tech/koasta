using System;

namespace Koasta.Service.ImageService.Models
{
    public class DtoImage
    {
        public int ImageId { get; set; }
        public int CompanyId { get; set; }
        public string ImageKey { get; set; }
        public string ImageTitle { get; set; }
        public Uri ImageUrl { get; set; }
    }
}
