using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using Koasta.Shared.PatchModels;
using Koasta.Shared.Queueing;
using Koasta.Shared.Types;
using Microsoft.Extensions.Logging;
using Shared.Queueing.Models;
using Square;
using Square.Exceptions;
using Square.Models;
using System;
using System.Threading.Tasks;

namespace Koasta.Service.EventService.Consumers
{
    public class RenewAccessTokenConsumer : AbstractQueueConsumer<Message<DtoRenewAccessTokenMessage>>
    {
        private readonly Constants _constants;
        private readonly ILoggerFactory _logger;
        private readonly ISettings _settings;
        private readonly CompanyRepository _companies;

        protected override string QueueName => _constants.AccessTokenRenewalQueueName;

        protected override string DeadletterQueueName => _constants.AccessTokenRenewalExchangeQueueName;

        protected override string DeadletterExchangeName => _constants.AccessTokenRenewalExchangeName;

        protected override string DeadletterRoutingKey => _constants.AccessTokenRenewalExchangeRoutingKey;

        protected override ISettings Settings => _settings;

        public RenewAccessTokenConsumer(
            ILoggerFactory logger,
            ISettings settings,
            Constants constants,
            CompanyRepository companies)
            :base(logger)
        {
            _logger = logger;
            _settings = settings;
            _constants = constants;
            _companies = companies;
        }

        protected override async Task HandleMessage(Message<DtoRenewAccessTokenMessage> message)
        {
            if (message == null || message.Data == null || 
                string.IsNullOrWhiteSpace(message.Data.ExternalAccessToken) ||
                string.IsNullOrWhiteSpace(message.Data.ExternalRefreshToken) ||
                message.Data.ExternalTokenExpiry == default(DateTime))
            {
                return;
            }

            try
            {
                var accessToken = message.Data.ExternalAccessToken;
                var refreshToken = message.Data.ExternalRefreshToken;
                var tokenExpiry = message.Data.ExternalTokenExpiry;
                var companyId = message.Data.CompanyId;

                var client = new SquareClient.Builder()
                .Environment(_settings.Connection.SquarePaymentsSandbox ? Square.Environment.Sandbox : Square.Environment.Production)
                .Build();

                var body = new ObtainTokenRequest(_settings.Connection.SquareAppId, _settings.Connection.SquareAccessToken, "refresh_token", refreshToken: refreshToken);

                ObtainTokenResponse response = null;
                try
                {
                    response = await client.OAuthApi.ObtainTokenAsync(body).ConfigureAwait(false);

                    accessToken = response.AccessToken;
                    refreshToken = response.RefreshToken;
                    var expiry = DateTime.TryParse(response.ExpiresAt, out DateTime parsedDate)? parsedDate : tokenExpiry;

                    if (!string.IsNullOrWhiteSpace(accessToken) &&
                        !string.IsNullOrWhiteSpace(refreshToken) && (expiry != null || expiry != default(DateTime)))
                    {
                        await _companies.UpdateCompany(new CompanyPatch
                        {
                            ResourceId = companyId,
                            ExternalAccessToken = new PatchOperation<string> { Operation = OperationKind.Update, Value = accessToken },
                            ExternalRefreshToken = new PatchOperation<string> { Operation = OperationKind.Update, Value = refreshToken },
                            ExternalTokenExpiry = new PatchOperation<DateTime> { Operation = OperationKind.Update, Value = expiry }
                        }).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"Unable to update company access token");
                }
            }
            catch (ApiException e)
            {
                Logger.LogError(e, $"Failed to connect to Square API");
            }
        }
    }
}
