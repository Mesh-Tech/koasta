using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Koasta.Service.Auth.Utils
{
    public class GoogleKey
    {
        public string Kty { get; set; }
        public string Kid { get; set; }
        public string Use { get; set; }
        public string Alg { get; set; }
        public string N { get; set; }
        public string E { get; set; }
    }

    internal class GoogleGetKeysResponse
    {
        public List<GoogleKey> Keys { get; set; }
    }

    public class GooglePublicKey
    {
        public GoogleKey RawKey { get; set; }
        public JsonWebKey JWKS { get; set; }
    }

    public class GoogleJWTKeyRefresher
    {
        public bool Ready { get; private set; }
        public GooglePublicKey[] Keys { get; private set; }
        private readonly WebRequestHelper webRequestHelper;
        private readonly ILogger logger;

        public GoogleJWTKeyRefresher(WebRequestHelper webRequestHelper, ILoggerFactory loggerFactory)
        {
            this.webRequestHelper = webRequestHelper;
            this.logger = loggerFactory.CreateLogger(nameof(GoogleJWTKeyRefresher));
        }

        public void Start()
        {
            Task.Run(async () => await Refresh().ConfigureAwait(false));
        }

        private async Task Refresh()
        {
            try
            {
                var response = await webRequestHelper.GetAsync<GoogleGetKeysResponse>("https://www.googleapis.com/oauth2/v3/certs").ConfigureAwait(false);
                if (response.Keys == null || response.Keys.Count == 0)
                {
                    logger.LogInformation("Failed to fetch Google's JWT signing keys. No keys were returned.");
                    return;
                }

                Keys = response.Keys.Select(k => new GooglePublicKey
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
                logger.LogInformation("Failed to fetch Google's JWT signing keys: {0}", ex.ToString());
            }
        }
    }
}
