using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Models
{
    public class ApiToken
    {
        [Column("apiTokenId")]
        public int ApiTokenId { get; set; }

        [Column("apiTokenValue")]
        public string ApiTokenValue { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("expiry")]
        public DateTime Expiry { get; set; }

        [Column("companyId")]
        public int? CompanyId { get; set; }
    }
}
