using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class Tag
    {
        [Column("tagId")]
        public int TagId { get; set; }
        [Column("tagName")]
        public string TagName { get; set; }
    }
}
