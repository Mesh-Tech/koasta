using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class Image
    {
        [Column("imageId")]
        public int ImageId { get; set; }
        [Column("companyId")]
        public int CompanyId { get; set; }
        [Column("imageKey")]
        public string ImageKey { get; set; }
        [Column("imageTitle")]
        public string ImageTitle { get; set; }
    }
}
