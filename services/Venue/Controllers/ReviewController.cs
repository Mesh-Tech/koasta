using System;
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
    public class ReviewController : Controller
    {
        private readonly ReviewRepository reviews;

        public ReviewController(ReviewRepository reviews)
        {
            this.reviews = reviews;
        }

        [HttpGet]
        [Route("{venueId}/user-reviews")]
        [ActionName("admin_fetch_reviews")]
        [ProducesResponseType(typeof(List<Review>), 200)]
        public async Task<IActionResult> AdminFetchReviews([FromRoute(Name = "venueId")] int venueId, [FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await reviews.FetchVenueReviews(venueId, page, count)
              .Ensure(r => r.HasValue, "Reviews were found")
              .OnBoth(r => r.IsFailure ? StatusCode(404, "") : StatusCode(200, r.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{venueId}/reviews")]
        [ActionName("fetch_reviews")]
        [ProducesResponseType(typeof(List<Review>), 200)]
        public async Task<IActionResult> FetchReviews([FromRoute(Name = "venueId")] int venueId, [FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await reviews.FetchVenueReviews(venueId, page, count)
              .Ensure(r => r.HasValue, "Reviews were found")
              .OnBoth(r => r.IsFailure ? StatusCode(404, "") : StatusCode(200, r.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("unmoderated-user-reviews")]
        [ActionName("admin_fetch_reviews")]
        [ProducesResponseType(typeof(List<Review>), 200)]
        public async Task<IActionResult> AdminFetchUnmoderatedReviews([FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            return await reviews.FetchUnmoderatedReviews(page, count)
              .Ensure(r => r.HasValue, "Reviews were found")
              .OnBoth(r => r.IsFailure ? StatusCode(404, "") : StatusCode(200, r.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [Route("{venueId}/user-reviews/{reviewId}")]
        [ActionName("admin_fetch_review")]
        [ProducesResponseType(typeof(Review), 200)]
        public async Task<IActionResult> AdminFetchReview([FromRoute(Name = "venueId")] int venueId, [FromRoute(Name = "reviewId")] int reviewId)
        {
            return await reviews.FetchVenueReview(venueId, reviewId)
              .Ensure(r => r.HasValue, "Review was found")
              .OnBoth(r => r.IsFailure ? StatusCode(404, "") : StatusCode(200, r.Value.Value))
              .ConfigureAwait(false);
        }

        [HttpPut]
        [Route("{venueId}/user-reviews/{reviewId}")]
        [ActionName("admin_update_review")]
        public async Task<IActionResult> AdminReplaceReview([FromRoute(Name = "venueId")] int venueId, [FromRoute(Name = "reviewId")] int reviewId, [FromBody] UpdatedReviewAdmin request)
        {
            var review = new Review
            {
                ReviewId = reviewId,
                VenueId = venueId,
                UserId = request.UserId,
                ReviewSummary = request.ReviewSummary,
                ReviewDetail = request.ReviewDetail,
                Rating = request.Rating,
                RegisteredInterest = request.RegisteredInterest,
                Approved = request.Approved,
            };
            return await reviews.ReplaceReview(review)
              .OnBoth(r => r.IsFailure ? StatusCode(500) : StatusCode(200))
              .ConfigureAwait(false);
        }

        [HttpPost]
        [Route("{venueId}/review")]
        [ActionName("create_review")]
        public Task<IActionResult> CreateReview([FromRoute(Name = "venueId")] int venueId, [FromBody] UpdatedReview request)
        {
            return CreateOrReplaceReview(venueId, request);
        }

        [HttpPut]
        [Route("{venueId}/review")]
        [ActionName("update_review")]
        public async Task<IActionResult> CreateOrReplaceReview([FromRoute(Name = "venueId")] int venueId, [FromBody] UpdatedReview request)
        {
            var existingReviewResult = await reviews.FetchVenueUserReview(venueId, this.GetAuthContext().User.Value.UserId).ConfigureAwait(false);
            if (existingReviewResult.IsFailure)
            {
                return StatusCode(500);
            }

            var existingReview = existingReviewResult.Value;
            Review review;
            var approved = false;
            if (existingReview.HasValue)
            {
                review = existingReview.Value;
                var existingSummary = review.ReviewSummary ?? "";
                var existingDetail = review.ReviewDetail ?? "";
                approved = existingSummary.Equals(request.ReviewSummary, StringComparison.Ordinal)
                  && existingDetail.Equals(request.ReviewDetail, StringComparison.Ordinal);
            }
            else
            {
                review = new Review
                {
                    VenueId = venueId,
                    UserId = this.GetAuthContext().User.Value.UserId,
                    ReviewSummary = request.ReviewSummary,
                    ReviewDetail = request.ReviewDetail,
                    Rating = request.Rating,
                    RegisteredInterest = request.RegisteredInterest,
                    Approved = approved,
                    Updated = DateTime.UtcNow,
                };
            }

            return await reviews.CreateOrReplaceReview(review)
              .OnBoth(r => r.IsFailure ? StatusCode(500) : StatusCode(200))
              .ConfigureAwait(false);
        }
    }
}
