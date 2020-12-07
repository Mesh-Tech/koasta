using System;

namespace Shared.Queueing.Models
{
    public class DtoRenewAccessTokenMessage
    {
        public int CompanyId { get; set; }

        public string ExternalAccessToken { get; set; }

        public string ExternalRefreshToken { get; set; }

        public DateTime ExternalTokenExpiry { get; set; }
    }
}
