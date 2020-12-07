using CSharpFunctionalExtensions;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Koasta.Shared.PatchModels;
using Koasta.Shared.Queueing;
using Koasta.Shared.Types;
using Microsoft.Extensions.Logging;
using Koasta.Shared.Queueing.Models;
using Square;
using Square.Exceptions;
using Square.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Koasta.Service.EventService.Consumers
{
    public class VenueSyncConsumer : AbstractQueueConsumer<Message<DtoVenueSyncMessage>>
    {
        private readonly Constants _constants;
        private readonly VenueRepository _venues;
        private readonly ISettings _settings;

        public VenueSyncConsumer(
            ILoggerFactory logger,
            ISettings settings,
            VenueRepository venues,
            Constants constants) : base(logger)
        {
            _settings = settings;
            _venues = venues;
            _constants = constants;
        }

        protected override string DeadletterQueueName => _constants.VenueSyncExchangeQueueName;

        protected override string DeadletterExchangeName => _constants.VenueSyncExchangeName;

        protected override string DeadletterRoutingKey => _constants.VenueSyncExchangeRoutingKey;

        protected override string QueueName => _constants.VenueSyncQueueName;

        protected override ISettings Settings => _settings;

        protected override async Task HandleMessage(Message<DtoVenueSyncMessage> message)
        {
            try
            {
                if (message != null || message?.Data != null || message.Data.Company != null)
                {
                    await SynchVenuesWithExternalPaymentProvider(message.Data.AccessToken, message.Data.Company).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }

        private async Task SynchVenuesWithExternalPaymentProvider(string accessToken, Company company)
        {
            if (company == null)
            {
                return;
            }

            try
            {
                var client = new SquareClient.Builder()
                .Environment(_settings.Connection.SquarePaymentsSandbox ? Square.Environment.Sandbox : Square.Environment.Production)
                .AccessToken(accessToken)
                .Build();

                ListLocationsResponse result = await client.LocationsApi.ListLocationsAsync().ConfigureAwait(false);

                var companyVenues = await _venues.FetchCompanyVenues(company.CompanyId, 0, int.MaxValue)
                                                .OnSuccess(c => c.Value.ToList())
                                                .ConfigureAwait(true);
                if (companyVenues.IsSuccess)
                {
                    foreach (var item in companyVenues.Value)
                    {
                        try
                        {
                            var matchedLocation = result.Locations
                                .FirstOrDefault(
                                x => NormaliseString(x.Name) == NormaliseString(item.VenueName) &&
                                NormaliseString(x.Address.PostalCode) == NormaliseString(item.VenuePostCode));

                            // need to find a match in Square location
                            if (matchedLocation != null)
                            {
                                if (string.IsNullOrWhiteSpace(item.ExternalLocationId) && matchedLocation != null)
                                {
                                    // We have a match and need to update local Database
                                    await _venues.UpdateVenue(new VenuePatch
                                    {
                                        ResourceId = item.VenueId,
                                        ExternalLocationId = new PatchOperation<string> { Operation = OperationKind.Update, Value = matchedLocation.Id },
                                    }).ConfigureAwait(false);
                                }
                            }
                            else if (matchedLocation == null)
                            {
                                // we have a location in our system that does not exist in sqaure. Need to add one
                                var createLocation = new CreateLocationRequest(new Location(
                                    name: item.VenueName,
                                    address:
                                    new Address(
                                        addressLine1: string.IsNullOrWhiteSpace(item.VenueAddress) ? "Not supplied" : item.VenueAddress,
                                        addressLine2: item.VenueAddress2,
                                        addressLine3: item.VenueAddress3,
                                        postalCode: string.IsNullOrWhiteSpace(item.VenuePostCode) ? "Not supplied" : item.VenuePostCode,
                                        locality: string.IsNullOrWhiteSpace(item.VenueCounty) ? "Not supplied" : item.VenueCounty,
                                        firstName: item.VenueContact),
                                    status: "ACTIVE",
                                    phoneNumber: GetVenuePhoneNumber(company, item)));

                                var newLocation = client.LocationsApi.CreateLocation(createLocation);

                                if (newLocation.Location != null)
                                {
                                    await _venues.UpdateVenue(new VenuePatch
                                    {
                                        ResourceId = item.VenueId,
                                        ExternalLocationId = new PatchOperation<string> { Operation = OperationKind.Update, Value = newLocation.Location.Id },
                                    }).ConfigureAwait(false);
                                }
                            }
                        }
                        catch (ApiException e)
                        {
                            Logger.LogError(e, $"Failed to create business location for venue {item.VenueName}");
                        }
                    }
                }
            }
            catch (ApiException e)
            {
                Logger.LogError(e, $"Failed to connect to Square API");
            }
        }

        private string GetVenuePhoneNumber(Company company, Venue venue)
        {
            string phoneNumber = null;

            if (venue != null && !string.IsNullOrWhiteSpace(venue.VenuePhone))
            {
                phoneNumber = venue.VenuePhone; ;
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                if (company != null && !string.IsNullOrWhiteSpace(company.CompanyPhone))
                {
                    phoneNumber = company.CompanyPhone;
                }
            }

            return phoneNumber;
        }

        private string NormaliseString(string toNormalise)
        {
            string result = string.Empty;

            if (!string.IsNullOrWhiteSpace(toNormalise))
            {
                result = toNormalise.ToLower().Replace(" ", "");
            }

            return result;
        }
    }
}
