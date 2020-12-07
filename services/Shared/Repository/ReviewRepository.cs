using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;

namespace Koasta.Shared.Database
{
    public partial class ReviewRepository
    {
        /// <summary>
        /// Fetches a single Review
        /// </summary>
        /// <param name="venueId">The id of the Venue whose Review you wish to fetch</param>
        /// <param name="resourceId">The id of the Review you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<Review>>> FetchVenueReview(int venueId, int resourceId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Review>("SELECT * FROM \"Review\" WHERE venueId = @VenueId AND reviewId = @ResourceId", new { VenueId = venueId, ResourceId = resourceId }).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<Review>.None);
                }

                return Result.Ok(Maybe<Review>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<Review>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches aggregated votes for all venues
        /// </summary>
        public async Task<Result<Maybe<List<AggregatedVenueVote>>>> FetchAggregatedVenueVotes()
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<AggregatedVenueVote>(@"select count(*) as total, v.venueid, v.venuename from ""Review"" r inner join ""Venue"" v on r.venueid = v.venueid  where r.registeredinterest = true group by v.venueid order by total desc").ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<AggregatedVenueVote>>.None);
                }

                return Result.Ok(Maybe<List<AggregatedVenueVote>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<AggregatedVenueVote>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches aggregated votes for all venues
        /// </summary>
        public async Task<Result<Maybe<List<int>>>> FetchUserVenueVotes(int userId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<int>(@"select venueid from ""Review"" where userid = @UserId", new { UserId = userId }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<int>>.None);
                }

                return Result.Ok(Maybe<List<int>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<int>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single Review
        /// </summary>
        /// <param name="venueId">The id of the Venue whose Review you wish to fetch</param>
        /// <param name="userId">The id of the User whose Review you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<Review>>> FetchVenueUserReview(int venueId, int userId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Review>("SELECT * FROM \"Review\" WHERE venueId = @VenueId AND userId = @UserId", new { VenueId = venueId, UserId = userId }).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<Review>.None);
                }

                return Result.Ok(Maybe<Review>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<Review>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a list of Reviews
        /// </summary>
        /// <param name="venueId">The id of the Venue whose Reviews you wish to fetch</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Review>>>> FetchVenueReviews(int venueId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Review>("SELECT * FROM \"Review\" WHERE venueId = @VenueId LIMIT @Limit OFFSET @Offset", new { VenueId = venueId, Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Review>>.None);
                }

                return Result.Ok(Maybe<List<Review>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Review>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a list of unmoderated Reviews
        /// </summary>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Review>>>> FetchUnmoderatedReviews(int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Review>("SELECT * FROM \"Review\" WHERE approved = false LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Review>>.None);
                }

                return Result.Ok(Maybe<List<Review>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Review>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Replaces a single Review with a new full set of values
        /// </summary>
        /// <param name="replacedReview">The new data for the Review you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> CreateOrReplaceReview(Review replacedReview)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync(
                  @"INSERT INTO ""Review"" (venueId, userId, reviewSummary, reviewDetail, rating, registeredInterest, updated, approved)
            VALUES (@VenueId, @UserId, @ReviewSummary, @ReviewDetail, @Rating, @RegisteredInterest, @Updated, @Approved)
            ON CONFLICT (venueId, userId) DO UPDATE
            SET reviewSummary = @ReviewSummary, reviewDetail = @ReviewDetail, rating = @Rating, registeredInterest = @RegisteredInterest, updated = @Updated, approved = @Approved
            RETURNING reviewId",
                  replacedReview
                ).ConfigureAwait(false);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }
    }
}
