using CSharpFunctionalExtensions;
using Koasta.Shared.Configuration;
using Koasta.Shared.Crypto;
using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.Extensions.Logging;
using Square;
using Square.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Koasta.Shared.Billing
{
    internal class BillingManager : IBillingManager
    {
        private readonly ISettings settings;
        private readonly ILogger logger;
        private readonly CompanyRepository companies;
        private readonly VenueRepository venues;
        private readonly ICryptoHelper cryptoHelper;

        public BillingManager(ISettings settings, CompanyRepository companies, VenueRepository venues, ILoggerFactory loggerFactory, ICryptoHelper cryptoHelper)
        {
            this.venues = venues;
            this.cryptoHelper = cryptoHelper;
            this.companies = companies;
            this.settings = settings;
            this.logger = loggerFactory.CreateLogger(nameof(BillingManager));
        }

        public Task<string> RegisterBillingAccount(Company company, BillingAccountDetails newDetails)
        {
            return Task.FromResult("");
        }

        public Task UpdateBillingAccount(Company company, BillingAccountDetails newDetails)
        {
            return Task.CompletedTask;
        }

        public Task<string> AddSubscriptionPackage(BillingSubscriptionPackage newPlan)
        {
            return Task.FromResult("");
        }

        public Task DeleteSubscriptionPackage(string planId)
        {
            return Task.CompletedTask;
        }

        public Task UpdateSubscriptionPackage(string planId, BillingSubscriptionPackage updatedPlan)
        {
            return Task.CompletedTask;
        }

        public Task<BillingSubscriptionPackage> GetSubscriptionPackage(string planId)
        {
            return Task.FromResult(new BillingSubscriptionPackage
            {
                Name = "UNIMPLEMENTED",
                MonthlyAmount = 1,
                Identifier = planId
            });
        }

        public Task<string> RegisterBillingCustomer(Company company, BillingAccountDetails newDetails)
        {
            return Task.FromResult("");
        }

        public Task UpdateBillingCustomer(Company company, BillingAccountDetails newDetails)
        {
            return Task.CompletedTask;
        }

        public Task<string> CreateSubscription(string customerId, List<string> packageIdentifiers)
        {
            throw new NotSupportedException();
        }

        public Task DeleteSubscription(string subscriptionId)
        {
            return Task.CompletedTask;
        }

        public async Task<string> ReserveOrderPayment(User user, BillingOrder order, string paymentSourceId, string destinationAccountId, string verificationReference)
        {
            var venue = await venues.FetchVenue(order.VenueId)
                .Ensure(v => v.HasValue, "Venue found")
                .OnSuccess(v => v.Value)
                .ConfigureAwait(false);

            if (!venue.IsSuccess)
            {
                logger.LogError($"Reserving payment failed for venueId {order.VenueId} as the venue was not found. Aborting.");
                throw new InvalidOperationException("Venue not found");
            }

            var company = await companies.FetchCompany(venue.Value.CompanyId)
                .Ensure(c => c.HasValue, "Company found")
                .OnSuccess(c => c.Value)
                .ConfigureAwait(false);

            if (!company.IsSuccess)
            {
                logger.LogError($"Reserving payment failed for venueId {order.VenueId} as the venue's company {venue.Value.CompanyId} was not found. Aborting.");
                throw new InvalidOperationException($"Company not found for venue id {order.VenueId}");
            }

            var externalAccessToken = await cryptoHelper.DecryptString(company.Value.ExternalAccessToken).ConfigureAwait(true);

            var client = new SquareClient.Builder()
                .Environment(settings.Connection.SquarePaymentsSandbox ? Square.Environment.Sandbox : Square.Environment.Production)
                .AccessToken(externalAccessToken)
                .Build();

            var amount = Convert.ToInt64(order.Total * 100) + Convert.ToInt64(order.KoastaFee * 100);
            var paymentRequestBuilder = new CreatePaymentRequest.Builder(paymentSourceId, Guid.NewGuid().ToString("N"), new Money.Builder().Amount(amount).Currency("GBP").Build())
                .Note($"User [{user.RegistrationId}] Payment for Order #{order.OrderNumber} at {order.VenueName}")
                .Autocomplete(false)
                .ReferenceId(order.OrderId.ToString(CultureInfo.InvariantCulture))
                .StatementDescriptionIdentifier($"Koasta Order {order.OrderNumber}")
                .LocationId(venue.Value.ExternalLocationId);

            CreatePaymentRequest paymentRequest;

            if (!string.IsNullOrWhiteSpace(verificationReference))
            {
                paymentRequest = paymentRequestBuilder
                    .VerificationToken(verificationReference)
                    .Build();
            }
            else
            {
                paymentRequest = paymentRequestBuilder.Build();
            }

            try
            {
                var payment = await client.PaymentsApi.CreatePaymentAsync(paymentRequest).ConfigureAwait(false);
                if (payment == null)
                {
                    logger.LogError($"Reserving payment failed for orderId {order.OrderId}. We received a null response from Square.");
                    throw new InvalidOperationException("Payment request failed. Null response.");
                }

                if ((payment.Errors?.Count ?? 0) > 0)
                {
                    logger.LogError($"Reserving payment failed for orderId {order.OrderId}. We received errors from Square: {string.Join('\n', payment.Errors)}");
                    throw new InvalidOperationException($"Payment request failed: {string.Join('\n', payment.Errors)}");
                }

                return payment.Payment.Id;
            }
            catch (Square.Exceptions.ApiException ex)
            {
                var errors = ex.Errors;

                // Painful dance to get the body so I can get the returned data.
                if (ex.HttpContext.Response is Square.Http.Response.HttpStringResponse httpStringResponse)
                {
                    logger.LogError($"Reserving payment failed for orderId {order.OrderId}. We received an error response from Square: {httpStringResponse.Body}");
                }
                else
                {
                    logger.LogError($"Reserving payment failed for orderId {order.OrderId}. We received errors from Square: {string.Join('\n', errors)}");
                }

                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw;
            }
        }

        public async Task FinaliseOrderPayment(Models.Order order)
        {
            if (string.IsNullOrWhiteSpace(order.ExternalPaymentId))
            {
                logger.LogWarning($"No external payment id found for order {order.OrderId}. Skipping finalise process.");
                return;
            }

            var venue = await venues.FetchVenue(order.VenueId)
                .Ensure(v => v.HasValue, "Venue found")
                .OnSuccess(v => v.Value)
                .ConfigureAwait(false);

            if (!venue.IsSuccess)
            {
                logger.LogError($"Finalising payment failed for venueId {order.VenueId} as the venue was not found. Aborting.");
                throw new InvalidOperationException("Venue not found");
            }

            var company = await companies.FetchCompany(venue.Value.CompanyId)
                .Ensure(c => c.HasValue, "Company found")
                .OnSuccess(c => c.Value)
                .ConfigureAwait(false);

            if (!company.IsSuccess)
            {
                logger.LogError($"Finalising payment failed for venueId {order.VenueId} as the venue's company {venue.Value.CompanyId} was not found. Aborting.");
                throw new InvalidOperationException($"Company not found for venue id {order.VenueId}");
            }

            var externalAccessToken = await cryptoHelper.DecryptString(company.Value.ExternalAccessToken).ConfigureAwait(true);

            var client = new SquareClient.Builder()
                .Environment(settings.Connection.SquarePaymentsSandbox ? Square.Environment.Sandbox : Square.Environment.Production)
                .AccessToken(externalAccessToken)
                .Build();

            try
            {
                await client.PaymentsApi.CompletePaymentAsync(order.ExternalPaymentId).ConfigureAwait(false);
            }
            catch (Square.Exceptions.ApiException ex)
            {
                var errors = ex.Errors;

                // Painful dance to get the body so I can get the returned data.
                if (ex.HttpContext.Response is Square.Http.Response.HttpStringResponse httpStringResponse)
                {
                    logger.LogError($"Reserving payment failed for orderId {order.OrderId}. We received an error response from Square: {httpStringResponse.Body}");
                }
                else
                {
                    logger.LogError($"Reserving payment failed for orderId {order.OrderId}. We received errors from Square: {string.Join('\n', errors)}");
                }

                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
                throw;
            }
        }

        public Task<string> CreateEphemeralKey(string customerId)
        {
            return Task.FromResult("");
        }

        public Task<string> CreateCustomer(User user)
        {
            return Task.FromResult("");
        }

        public Task UpdateSubscription(string customerId, string subscriptionId, List<string> packageIdentifiers)
        {
            return Task.CompletedTask;
        }
    }
}
