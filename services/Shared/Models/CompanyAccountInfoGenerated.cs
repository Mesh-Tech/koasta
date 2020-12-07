using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class CompanyAccountInfo
    {
        [Column("externalAccountId")]
        public string ExternalAccountId { get; set; }
        [Column("externalCustomerId")]
        public string ExternalCustomerId { get; set; }
    }
}
