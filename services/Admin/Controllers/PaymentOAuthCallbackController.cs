using CSharpFunctionalExtensions;
using Koasta.Shared.Configuration;
using Koasta.Shared.Crypto;
using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Koasta.Shared.PatchModels;
using Koasta.Shared.Queueing;
using Koasta.Shared.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Koasta.Shared.Queueing.Models;
using Square;
using Square.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Koasta.Service.Admin.Controllers
{
    [Route("/payments/oauth")]
    public class PaymentOAuthCallbackController : Controller
    {
        private readonly CompanyRepository companies;
        private readonly EmployeeRepository employees;
        private readonly EmployeeRoleRepository roles;
        private readonly Constants constants;
        private readonly ISettings settings;
        private readonly IDistributedCache cache;
        private readonly ICryptoHelper cryptoHelper;
        private readonly IMessagePublisher messagePublisher;
        private readonly ILogger logger;

        public PaymentOAuthCallbackController(CompanyRepository companies,
                                              EmployeeRepository employees,
                                              EmployeeRoleRepository roles,
                                              Constants constants,
                                              ISettings settings,
                                              ICryptoHelper cryptoHelper,
                                              IDistributedCache cache,
                                              IMessagePublisher messagePublisher,
                                              ILoggerFactory loggerFactory)
        {
            this.companies = companies;
            this.employees = employees;
            this.roles = roles;
            this.constants = constants;
            this.settings = settings;
            this.cryptoHelper = cryptoHelper;
            this.cache = cache;
            this.messagePublisher = messagePublisher;
            this.logger = loggerFactory.CreateLogger(nameof(PaymentOAuthCallbackController));
        }

        [HttpGet]
        public async Task<IActionResult> SubmitToken([FromQuery] string code, [FromQuery] string response_type, [FromQuery] string state)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(response_type) || string.IsNullOrWhiteSpace(state) || !string.Equals(response_type, "code", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/Index");
            }

            var employeeId = await cache.GetStringAsync($"company-square-verify-{state}").ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(employeeId))
            {
                return RedirectToPage("/Index");
            }

            int rawEmployeeId = -1;
            if (!int.TryParse(employeeId, out rawEmployeeId))
            {
                return RedirectToPage("/Index");
            }

            if (rawEmployeeId == -1)
            {
                return RedirectToPage("/Index");
            }

            var employee = await employees.FetchEmployee(rawEmployeeId)
                .Ensure(e => e.HasValue, "Employee found")
                .OnSuccess(e => e.Value)
                .ConfigureAwait(false);

            if (employee.IsFailure)
            {
                return RedirectToPage("/Index");
            }

            var role = await roles.FetchEmployeeRole(employee.Value.RoleId)
                .Ensure(r => r.HasValue, "Role found")
                .OnSuccess(r => r.Value)
                .ConfigureAwait(false);

            if (role.IsFailure || !role.Value.CanAdministerCompany)
            {
                return RedirectToPage("/Index");
            }

            var company = await companies.FetchCompany(employee.Value.CompanyId)
                .Ensure(c => c.HasValue, "Company found")
                .OnSuccess(c => c.Value)
                .ConfigureAwait(false);

            if (company.IsFailure)
            {
                return RedirectToPage("/Index");
            }

            var client = new SquareClient.Builder()
                .Environment(settings.Connection.SquarePaymentsSandbox ? Square.Environment.Sandbox : Square.Environment.Production)
                .Build();

            var body = new ObtainTokenRequest(settings.Connection.SquareAppId, settings.Connection.SquareAccessToken, "authorization_code", code);

            ObtainTokenResponse response = null;
            try
            {
                response = await client.OAuthApi.ObtainTokenAsync(body).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return RedirectToPage("/Index");
            }

            if (response == null)
            {
                return RedirectToPage("/Index");
            }

            var accessToken = response.AccessToken;
            var refreshToken = response.RefreshToken;
            var expiresAt = response.ExpiresAt;
            var merchantId = response.MerchantId;

            DateTime tokenExpiry;
            if (!DateTime.TryParse(response.ExpiresAt, out tokenExpiry))
            {
                return RedirectToPage("/Index");
            }

            var companyId = company.Value.CompanyId;

            SynchVenuesWithExternalPaymentProvider(accessToken, company.Value);

            accessToken = await EncryptString(accessToken).ConfigureAwait(false);
            refreshToken = await EncryptString(refreshToken).ConfigureAwait(false);

            return await companies.UpdateCompany(new CompanyPatch
            {
                ResourceId = companyId,
                ExternalAccessToken = new PatchOperation<string> { Operation = OperationKind.Update, Value = accessToken },
                ExternalRefreshToken = new PatchOperation<string> { Operation = OperationKind.Update, Value = refreshToken },
                ExternalAccountId = new PatchOperation<string> { Operation = OperationKind.Update, Value = merchantId },
                ExternalTokenExpiry = new PatchOperation<DateTime> { Operation = OperationKind.Update, Value = tokenExpiry }
            }).OnBoth(u => RedirectToPage("/Index")).ConfigureAwait(false);
        }

        private async Task<string> EncryptString(string toEncrypt)
        {
            if (!string.IsNullOrWhiteSpace(toEncrypt))
            {
                return await cryptoHelper.EncryptString(toEncrypt).ConfigureAwait(false);
            }

            return toEncrypt;
        }

        private void SynchVenuesWithExternalPaymentProvider(string accessToken, Company company)
        {
            if (company == null)
            {
                return;
            }

            try
            {
                messagePublisher.DirectPublish(
                    constants.VenueSyncQueueName,
                    constants.VenueSyncExchangeRoutingKey,
                    constants.VenueSyncExchangeName,
                    constants.VenueSyncExchangeQueueName,
                    new Message<DtoVenueSyncMessage>
                    {
                        Type = constants.VenueSynch,
                        Data = new DtoVenueSyncMessage(Guid.NewGuid(), VenueSyncAction.Create, accessToken, company)
                    });

            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }
        }
    }
}
