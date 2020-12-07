using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class Document
    {
        [Column("documentId")]
        public int DocumentId { get; set; }
        [Column("companyId")]
        public int CompanyId { get; set; }
        [Column("documentKey")]
        public string DocumentKey { get; set; }
        [Column("documentTitle")]
        public string DocumentTitle { get; set; }
        [Column("documentDescription")]
        public string DocumentDescription { get; set; }
    }
}
