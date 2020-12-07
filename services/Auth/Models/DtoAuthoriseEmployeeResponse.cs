using Koasta.Shared.Models;
using System;

namespace Koasta.Service.Auth.Models
{
    public class DtoAuthoriseEmployeeResponse
    {
        public string AuthToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiry { get; set; }
        public DateTime RefreshExpiry { get; set; }

        public static explicit operator DtoAuthoriseEmployeeResponse(EmployeeSession session)
        {
            return new DtoAuthoriseEmployeeResponse
            {
                AuthToken = session.AuthToken,
                RefreshToken = session.RefreshToken,
                Expiry = session.Expiry,
                RefreshExpiry = session.RefreshExpiry
            };
        }

        public DtoAuthoriseEmployeeResponse ToDtoAuthoriseEmployeeResponse()
        {
            return new DtoAuthoriseEmployeeResponse
            {
                AuthToken = AuthToken,
                RefreshToken = RefreshToken,
                Expiry = Expiry,
                RefreshExpiry = RefreshExpiry
            };
        }
    }
}
