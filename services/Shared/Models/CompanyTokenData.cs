using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Models
{
    public class CompanyTokenData
    {
        [Column("companyId")]
        public int CompanyId { get; set; }

        [Column("externalaccesstoken")]
        public string ExternalAccessToken { get; set; }

        [Column("externalrefreshtoken")]
        public string ExternalRefreshToken { get; set; }

        [Column("externaltokenexpiry")]
        public DateTime ExternalTokenExpiry { get; set; }
    }
}
