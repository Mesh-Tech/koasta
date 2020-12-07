using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Models;
using System.Collections.Generic;

namespace Koasta.Service.VenueService.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/venue")]
    public class VenueController : Controller
    {
        private readonly VenueRepository venues;

        public VenueController(VenueRepository venues)
        {
            this.venues = venues;
        }

        [HttpGet]
        [ActionName("fetch_venues")]
        [ProducesResponseType(typeof(List<Venue>), 200)]
        public async Task<IActionResult> FetchVenues([FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await venues.FetchOrderedVenues(page, count)
              .Ensure(v => v.HasValue, "Venues were found")
              .OnBoth(v => v.IsFailure ? StatusCode(404, "") : StatusCode(200, v.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_venue")]
        [Route("{venueId}")]
        [ProducesResponseType(typeof(Venue), 200)]
        public async Task<IActionResult> FetchVenue([FromRoute(Name = "venueId")] int venueId)
        {
            return await venues.FetchFullVenue(venueId)
              .Ensure(v => v.HasValue, "Venue was found")
              .OnBoth(v => v.IsFailure ? StatusCode(404, "") : StatusCode(200, v.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpDelete]
        [ActionName("delete_venue")]
        [Route("{venueId}")]
        public async Task<IActionResult> DropVenue([FromRoute(Name = "venueId")] int venueId)
        {
            var venue = await venues.FetchVenue(venueId).ConfigureAwait(false);
            if (venue.IsFailure || venue.Value.HasNoValue)
            {
                return NotFound();
            }

            var ven = venue.Value.Value;
            if (this.GetAuthContext().Employee.Value.CompanyId != ven.CompanyId && !this.GetAuthContext().EmployeeRole.Value.CanAdministerSystem)
            {
                return Unauthorized();
            }

            return await venues.DropVenue(venueId)
              .OnBoth(v => v.IsFailure ? StatusCode(404) : StatusCode(200))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_company_venues")]
        [Route("{companyId}/venues")]
        [ProducesResponseType(typeof(List<Venue>), 200)]
        public async Task<IActionResult> FetchCompanyVenues([FromRoute(Name = "companyId")] int companyId, [FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await venues.FetchCompanyVenues(companyId, page, count)
              .Ensure(v => v.HasValue, "Venues were found")
              .OnBoth(v => v.IsFailure ? StatusCode(404, "") : StatusCode(200, v.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_nearby_venues")]
        [Route("nearby-venues/{lat}/{lon}")]
        [ProducesResponseType(typeof(List<Venue>), 200)]
        public async Task<IActionResult> FetchNearbyVenues([FromRoute(Name = "lat")] double lat, [FromRoute(Name = "lon")] double lon, [FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await venues.FetchNearbyVenues(lat, lon, page, count)
              .Ensure(v => v.HasValue, "Venues were found")
              .OnBoth(v => v.IsFailure ? StatusCode(404, "") : StatusCode(200, v.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_queried_venues")]
        [Route("named/{query}")]
        [ProducesResponseType(typeof(List<Venue>), 200)]
        public async Task<IActionResult> FetchQueriedVenues([FromRoute(Name = "query")] string query, [FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20, [FromQuery(Name = "includeDetails")] bool includeDetails = true)
        {
            return await (includeDetails ? venues.FetchQueriedVenues(query, page, count) : venues.FetchSimpleQueriedVenues(query, page, count))
              .Ensure(v => v.HasValue, "Venues were found")
              .OnBoth(v => v.IsFailure ? StatusCode(404, "") : StatusCode(200, v.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [ActionName("create_venue")]
        public async Task<IActionResult> CreateVenue([FromBody] NewVenue venue)
        {
            var newVenue = new Venue
            {
                CompanyId = venue.CompanyId,
                VenueName = venue.VenueName,
                VenueCode = venue.VenueCode,
                VenueAddress = venue.VenueAddress,
                VenuePostCode = venue.VenuePostCode,
                VenueContact = venue.VenueContact,
                VenueDescription = venue.VenueDescription,
                VenueNotes = venue.VenueNotes,
                VenueLatitude = venue.VenueLatitude,
                VenueLongitude = venue.VenueLongitude,
                ImageId = venue.ImageId,
                VenuePhone = venue.VenuePhone,
            };

            return await venues.CreateVenue(newVenue)
              .Ensure(v => v.HasValue, "Venue created successfully")
              .OnBoth(v => v.IsFailure ? StatusCode(500) : StatusCode(201))
              .ConfigureAwait(false);
        }

        [HttpPut]
        [ActionName("update_venue")]
        [Route("{venueId}")]
        public async Task<IActionResult> ReplaceVenue([FromRoute(Name = "venueId")] int venueId, [FromBody] NewVenue venue)
        {
            var newVenue = new Venue
            {
                VenueId = venueId,
                CompanyId = venue.CompanyId,
                VenueName = venue.VenueName,
                VenueCode = venue.VenueCode,
                VenueAddress = venue.VenueAddress,
                VenuePostCode = venue.VenuePostCode,
                VenueContact = venue.VenueContact,
                VenueDescription = venue.VenueDescription,
                VenueNotes = venue.VenueNotes,
                VenueLatitude = venue.VenueLatitude,
                VenueLongitude = venue.VenueLongitude,
                ImageId = venue.ImageId,
                VenuePhone = venue.VenuePhone,
            };

            return await venues.ReplaceVenue(newVenue)
              .OnBoth(v => v.IsFailure ? StatusCode(500) : StatusCode(200))
              .ConfigureAwait(false);
        }
    }
}
