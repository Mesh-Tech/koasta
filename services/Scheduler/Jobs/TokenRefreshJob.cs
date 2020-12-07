using Hangfire;
using Koasta.Shared.Database;
using Koasta.Shared.Queueing;
using Koasta.Shared.Types;
using Microsoft.Extensions.Logging;
using Shared.Queueing.Models;
using System;
using System.Threading.Tasks;

namespace Koasta.Service.Scheduler.Jobs
{
    public class TokenRefreshJob : ITokenRefreshJob
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly CompanyRepository _companyRepository;
        private readonly ILogger _logger;
        private readonly Constants _constants;

        public string Name => "Access token refresh job";

        // Trigger event at 7am every morning. Guessing this  
        // would be a quiet time to do maintenance
        public string TriggerTime => Cron.Daily(7);

        public TokenRefreshJob(
            IMessagePublisher messagePublisher,
            Constants constants,
            CompanyRepository companyRepository,
            ILoggerFactory loggerFactory)
        {
            _messagePublisher = messagePublisher;
            _constants = constants;
            _companyRepository = companyRepository;
            _logger = loggerFactory.CreateLogger(nameof(TokenRefreshJob));
        }

        public async Task ProcessJob()
        {
            var expiringCompanyTokensResult = await _companyRepository.GetExipringCompanyTokens(4).ConfigureAwait(true);

            if (expiringCompanyTokensResult.IsSuccess)
            {
                foreach (var token in expiringCompanyTokensResult.Value.Value)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(token.ExternalAccessToken) &&
                            !string.IsNullOrWhiteSpace(token.ExternalRefreshToken))
                        {

                            _messagePublisher.DirectPublish(
                                _constants.AccessTokenRenewalQueueName,
                                _constants.AccessTokenRenewalExchangeRoutingKey,
                                _constants.AccessTokenRenewalExchangeName,
                                _constants.AccessTokenRenewalExchangeQueueName,
                                new Message<DtoRenewAccessTokenMessage>
                                {
                                    Type = _constants.AccessTokenRenewal,
                                    Data = new DtoRenewAccessTokenMessage
                                    {
                                        CompanyId = token.CompanyId,
                                        ExternalAccessToken = token.ExternalAccessToken,
                                        ExternalRefreshToken = token.ExternalRefreshToken,
                                        ExternalTokenExpiry = token.ExternalTokenExpiry
                                    }
                                });
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.ToString());
                    }
                }
            }
        }
    }
}
