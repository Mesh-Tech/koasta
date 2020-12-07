using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Koasta.Service.Auth.Utils
{
    public class AppleKey
    {
        public string Kty { get; set; }
        public string Kid { get; set; }
        public string Use { get; set; }
        public string Alg { get; set; }
        public string N { get; set; }
        public string E { get; set; }
    }

    internal class AppleGetKeysResponse
    {
        public List<AppleKey> Keys { get; set; }
    }

    public class ApplePublicKey
    {
        public AppleKey RawKey { get; set; }
        public JsonWebKey JWKS { get; set; }
    }

    public class AppleJWTKeyRefresher
    {
        public bool Ready { get; private set; }
        public ApplePublicKey[] Keys { get; private set; }
        private readonly WebRequestHelper webRequestHelper;
        private readonly ILogger logger;

        public AppleJWTKeyRefresher(WebRequestHelper webRequestHelper, ILoggerFactory loggerFactory)
        {
            this.webRequestHelper = webRequestHelper;
            this.logger = loggerFactory.CreateLogger(nameof(AppleJWTKeyRefresher));
        }

        public void Start()
        {
            Task.Run(async () => await Refresh().ConfigureAwait(false));
        }

        private async Task Refresh()
        {
            try
            {
                var response = await webRequestHelper.GetAsync<AppleGetKeysResponse>("https://appleid.apple.com/auth/keys").ConfigureAwait(false);
                if (response.Keys == null || response.Keys.Count == 0)
                {
                    logger.LogInformation("Failed to fetch Apple's JWT signing keys. No keys were returned.");
                    return;
                }

                Keys = response.Keys.Select(k => new ApplePublicKey
                {
                    JWKS = new JsonWebKey
                    {
                        Kty = k.Kty,
                        Kid = k.Kid,
                        Use = k.Use,
                        Alg = k.Alg,
                        N = k.N,
                        E = k.E,
                    },
                    RawKey = k
                }).ToArray();
                Ready = true;
            }
            catch (Exception ex)
            {
                logger.LogInformation("Failed to fetch Apple's JWT signing keys: {0}", ex.ToString());
            }
        }
    }
}
