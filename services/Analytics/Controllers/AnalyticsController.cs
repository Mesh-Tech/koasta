using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Koasta.Shared.Middleware;
using Koasta.Shared.Repository;
using System.Threading.Tasks;
using System.Collections.Generic;
using Koasta.Shared.Models;

namespace Koasta.Service.AnalyticsService.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/analytics")]
    public class AnalyticsController : Controller
    {
        private readonly AnalyticsRepository _repository;

        public AnalyticsController(AnalyticsRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [Route("averagespend/{companyId}")]
        [ActionName("get_average_order_amount")]
        [ProducesResponseType(typeof(List<AverageOrderValue>), 200)]
        public async Task<IActionResult> GetAverageOrderAmount([FromRoute(Name = "companyId")] int compnayId)
        {
            return await _repository.GetAverageOrderAmount(compnayId)
              .Ensure(m => m.HasValue, "Have Values")
              .OnBoth(m => m.IsFailure ? StatusCode(404, "") : StatusCode(200, m.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("outstanding/{venueId}")]
        [ActionName("get_outstanding_orders")]
        [ProducesResponseType(typeof(TotalOrderStatus), 200)]
        public async Task<IActionResult> GetOutstandingOrders([FromRoute(Name = "venueId")] int venueId)
        {
            return await _repository.GetActiveOrderStatusForVenue(venueId)
              .Ensure(m => m.HasValue, "Have Values")
              .OnBoth(m => m.IsFailure ? StatusCode(404, "") : StatusCode(200, m.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("completed/{venueId}")]
        [ActionName("get_completed_orders")]
        [ProducesResponseType(typeof(CompletedTotalOrderStatus), 200)]
        public async Task<IActionResult> GetCompletedOrders([FromRoute(Name = "venueId")] int venueId)
        {
            return await _repository.GetCompletedOrderStatusForVenue(venueId, (int)OrderStatus.Complete)
              .Ensure(m => m.HasValue, "Have Values")
              .OnBoth(m => m.IsFailure ? StatusCode(404, "") : StatusCode(200, m.Value.Value))
              .ConfigureAwait(false);
        }
    }
}
