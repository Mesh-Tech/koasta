using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class ProductType
    {
        [Column("productTypeId")]
        public int ProductTypeId { get; set; }
        [Column("productTypeName")]
        public string ProductTypeName { get; set; }
    }
}
