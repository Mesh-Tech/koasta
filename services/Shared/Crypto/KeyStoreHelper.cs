using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Koasta.Shared.Crypto
{
    public class KeyStoreHelper : IKeyStoreHelper
    {
        private ILogger logger;

        public KeyStoreHelper(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger(nameof(KeyStoreHelper));
        }

        public async Task<string> GetKeyParameterValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

            try
            {
                using (var client = new AmazonSimpleSystemsManagementClient(RegionEndpoint.EUWest1))
                {
                    var response =
                        await client.GetParameterAsync(
                            new GetParameterRequest
                            {
                                Name = key,
                                WithDecryption = false
                            }).ConfigureAwait(true);

                    return response.Parameter.Value;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get parameter value");
                return string.Empty;
            }
        }
    }
}
