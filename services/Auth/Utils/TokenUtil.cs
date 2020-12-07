using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Koasta.Shared.Configuration;
using Koasta.Shared.Models;

namespace Koasta.Service.Auth.Utils
{
    public class TokenMetadata
    {
        public DateTime Expiry { get; set; }
        public string AuthIdentifier { get; set; }
    }

    internal class FacebookVerificationResponse
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
    }

    public class TokenUtil
    {
        private readonly ISettings settings;
        private readonly WebRequestHelper webRequestHelper;
        private readonly AppleJWTKeyRefresher appleJWTKeyRefresher;
        private readonly GoogleJWTKeyRefresher googleJWTKeyRefresher;

        public TokenUtil(ISettings settings, WebRequestHelper webRequestHelper, AppleJWTKeyRefresher appleJWTKeyRefresher, GoogleJWTKeyRefresher googleJWTKeyRefresher)
        {
            this.webRequestHelper = webRequestHelper;
            this.settings = settings;
            this.appleJWTKeyRefresher = appleJWTKeyRefresher;
            this.googleJWTKeyRefresher = googleJWTKeyRefresher;
        }

        public string GenerateToken() => Guid.NewGuid().ToString();

        public string GenerateNumericalToken()
        {
            const int min = 100000;
            const int max = 999999;
            var random = new Random();
            return random.Next(min, max).ToString(CultureInfo.InvariantCulture);
        }

        public async Task<TokenMetadata> GetMetadataForToken(string token, AuthType authType)
        {
            if (authType == AuthType.Unknown)
            {
                return null;
            }

            if (authType == AuthType.Apple)
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                try
                {
                    var unverifiedToken = tokenHandler.ReadJwtToken(token);
                    if (unverifiedToken == null || unverifiedToken.Header == null || unverifiedToken.Header.Kid == null)
                    {
                        return null;
                    }

                    var expectedKid = unverifiedToken.Header.Kid;

                    var publicKey = Array.Find(appleJWTKeyRefresher.Keys, k => k.RawKey.Kid.Equals(expectedKid, StringComparison.Ordinal));
                    if (publicKey == null)
                    {
                        return null;
                    }

                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = "https://appleid.apple.com",
                        ValidAudience = "io.meshtech.pubcrawl",
                        IssuerSigningKey = publicKey.JWKS
                    }, out SecurityToken validatedToken);

                    if (!(validatedToken is JwtSecurityToken))
                    {
                        return null;
                    }

                    var actualToken = (JwtSecurityToken)validatedToken;

                    if (DateTime.UtcNow.CompareTo(actualToken.ValidTo) >= 0)
                    {
                        return null;
                    }

                    if (string.IsNullOrWhiteSpace(actualToken.Subject))
                    {
                        return null;
                    }

                    return new TokenMetadata
                    {
                        Expiry = DateTime.MaxValue,
                        AuthIdentifier = actualToken.Subject
                    };
                }
                catch
                {
                    return null;
                }
            }

            if (authType == AuthType.Facebook)
            {
                var facebookAddress = settings.Auth.FacebookAuthVerifyAddress;
                var url = string.Format(CultureInfo.InvariantCulture, facebookAddress, token);

                try
                {
                    var result = await webRequestHelper.GetAsync<FacebookVerificationResponse>(url).ConfigureAwait(false);
                    if (result == null || string.IsNullOrWhiteSpace(result.Id))
                    {
                        return null;
                    }

                    return new TokenMetadata
                    {
                        Expiry = DateTime.MaxValue,
                        AuthIdentifier = result.Id
                    };
                }
                catch (Exception)
                {
                    return null;
                }
            }
            if (authType == AuthType.Google)
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                try
                {
                    var unverifiedToken = tokenHandler.ReadJwtToken(token);
                    if (unverifiedToken == null || unverifiedToken.Header == null || unverifiedToken.Header.Kid == null)
                    {
                        return null;
                    }

                    var expectedKid = unverifiedToken.Header.Kid;

                    var publicKey = Array.Find(googleJWTKeyRefresher.Keys, k => k.RawKey.Kid.Equals(expectedKid, StringComparison.Ordinal));
                    if (publicKey == null)
                    {
                        return null;
                    }

                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = "https://accounts.google.com",
                        ValidAudience = "762910246304-t499hbjbsqnjfvul0fdr1n2h0sdt9md2.apps.googleusercontent.com",
                        IssuerSigningKey = publicKey.JWKS
                    }, out SecurityToken validatedToken);

                    if (!(validatedToken is JwtSecurityToken))
                    {
                        return null;
                    }

                    var actualToken = (JwtSecurityToken)validatedToken;

                    if (DateTime.UtcNow.CompareTo(actualToken.ValidTo) >= 0)
                    {
                        return null;
                    }

                    if (string.IsNullOrWhiteSpace(actualToken.Subject))
                    {
                        return null;
                    }

                    return new TokenMetadata
                    {
                        Expiry = DateTime.MaxValue,
                        AuthIdentifier = actualToken.Subject
                    };
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }
}
