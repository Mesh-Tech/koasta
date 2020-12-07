using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Koasta.Shared.Database
{
    public partial class VenueOpeningTimeRepository
    {
        /// <summary>
        /// Fetches multiple VenueOpeningTimes
        /// </summary>
        /// <param name="venueId">The venue id to query against</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<VenueOpeningTime>>>> FetchVenueVenueOpeningTimes(int venueId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<VenueOpeningTime>("SELECT * FROM \"VenueOpeningTime\" WHERE venueId = @VenueId", new { VenueId = venueId }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<VenueOpeningTime>>.None);
                }

                return Result.Ok(Maybe<List<VenueOpeningTime>>.From(data));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<List<VenueOpeningTime>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Upserts venue opening times
        /// </summary>
        public async Task<Result> UpsertVenueOpeningTimes
        (
            int venueId,
            TimeSpan? SundayOpeningTimeStart,
            TimeSpan? SundayOpeningTimeEnd,
            TimeSpan? MondayOpeningTimeStart,
            TimeSpan? MondayOpeningTimeEnd,
            TimeSpan? TuesdayOpeningTimeStart,
            TimeSpan? TuesdayOpeningTimeEnd,
            TimeSpan? WednesdayOpeningTimeStart,
            TimeSpan? WednesdayOpeningTimeEnd,
            TimeSpan? ThursdayOpeningTimeStart,
            TimeSpan? ThursdayOpeningTimeEnd,
            TimeSpan? FridayOpeningTimeStart,
            TimeSpan? FridayOpeningTimeEnd,
            TimeSpan? SaturdayOpeningTimeStart,
            TimeSpan? SaturdayOpeningTimeEnd
        )
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.OpenAsync().ConfigureAwait(false);
                var transaction = con.BeginTransaction();
                const string query = @"INSERT INTO ""VenueOpeningTime"" (venueId, startTime, endTime, dayOfWeek)
                    VALUES (@VenueId, @StartTime, @EndTime, @DayOfWeek)
                    ON CONFLICT (venueId, dayOfWeek)
                    DO UPDATE SET startTime = @StartTime, endTime = @EndTime";

                if (SundayOpeningTimeStart != null && SundayOpeningTimeEnd != null)
                {
                    await con.ExecuteAsync(query, new { @DayOfWeek = 0, @StartTime = SundayOpeningTimeStart.Value, @EndTime = SundayOpeningTimeEnd.Value, @VenueId = venueId }).ConfigureAwait(false);
                }
                else
                {
                    await con.ExecuteAsync(@"DELETE FROM ""VenueOpeningTime"" WHERE venueId = @VenueId AND dayOfWeek = 0", new { VenueId = venueId }).ConfigureAwait(false);
                }

                if (MondayOpeningTimeStart != null && MondayOpeningTimeEnd != null)
                {
                    await con.ExecuteAsync(query, new { @DayOfWeek = 1, @StartTime = MondayOpeningTimeStart.Value, @EndTime = MondayOpeningTimeEnd.Value, @VenueId = venueId }).ConfigureAwait(false);
                }
                else
                {
                    await con.ExecuteAsync(@"DELETE FROM ""VenueOpeningTime"" WHERE venueId = @VenueId AND dayOfWeek = 1", new { VenueId = venueId }).ConfigureAwait(false);
                }

                if (TuesdayOpeningTimeStart != null && TuesdayOpeningTimeEnd != null)
                {
                    await con.ExecuteAsync(query, new { @DayOfWeek = 2, @StartTime = TuesdayOpeningTimeStart.Value, @EndTime = TuesdayOpeningTimeEnd.Value, @VenueId = venueId }).ConfigureAwait(false);
                }
                else
                {
                    await con.ExecuteAsync(@"DELETE FROM ""VenueOpeningTime"" WHERE venueId = @VenueId AND dayOfWeek = 2", new { VenueId = venueId }).ConfigureAwait(false);
                }

                if (WednesdayOpeningTimeStart != null && WednesdayOpeningTimeEnd != null)
                {
                    await con.ExecuteAsync(query, new { @DayOfWeek = 3, @StartTime = WednesdayOpeningTimeStart.Value, @EndTime = WednesdayOpeningTimeEnd.Value, @VenueId = venueId }).ConfigureAwait(false);
                }
                else
                {
                    await con.ExecuteAsync(@"DELETE FROM ""VenueOpeningTime"" WHERE venueId = @VenueId AND dayOfWeek = 3", new { VenueId = venueId }).ConfigureAwait(false);
                }

                if (ThursdayOpeningTimeStart != null && ThursdayOpeningTimeEnd != null)
                {
                    await con.ExecuteAsync(query, new { @DayOfWeek = 4, @StartTime = ThursdayOpeningTimeStart.Value, @EndTime = ThursdayOpeningTimeEnd.Value, @VenueId = venueId }).ConfigureAwait(false);
                }
                else
                {
                    await con.ExecuteAsync(@"DELETE FROM ""VenueOpeningTime"" WHERE venueId = @VenueId AND dayOfWeek = 4", new { VenueId = venueId }).ConfigureAwait(false);
                }

                if (FridayOpeningTimeStart != null && FridayOpeningTimeEnd != null)
                {
                    await con.ExecuteAsync(query, new { @DayOfWeek = 5, @StartTime = FridayOpeningTimeStart.Value, @EndTime = FridayOpeningTimeEnd.Value, @VenueId = venueId }).ConfigureAwait(false);
                }
                else
                {
                    await con.ExecuteAsync(@"DELETE FROM ""VenueOpeningTime"" WHERE venueId = @VenueId AND dayOfWeek = 5", new { VenueId = venueId }).ConfigureAwait(false);
                }

                if (SaturdayOpeningTimeStart != null && SaturdayOpeningTimeEnd != null)
                {
                    await con.ExecuteAsync(query, new { @DayOfWeek = 6, @StartTime = SaturdayOpeningTimeStart.Value, @EndTime = SaturdayOpeningTimeEnd.Value, @VenueId = venueId }).ConfigureAwait(false);
                }
                else
                {
                    await con.ExecuteAsync(@"DELETE FROM ""VenueOpeningTime"" WHERE venueId = @VenueId AND dayOfWeek = 6", new { VenueId = venueId }).ConfigureAwait(false);
                }

                await transaction.CommitAsync().ConfigureAwait(false);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }
    }
}
