using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;
using Koasta.Shared.Types;
using Koasta.Shared.Helpers;

namespace Koasta.Shared.Database
{
    public partial class VenueRepository : RepositoryBase<Venue>
    {
        /// <summary>
        /// Fetches multiple Venues
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Venue>>>> FetchOrderedVenues(int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Venue>(@"SELECT * FROM ""Venue"" ORDER BY venueName ASC LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Venue>>.None);
                }

                return Result.Ok(Maybe<List<Venue>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Venue>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Venues for a given company id
        /// </summary>
        /// <param name="companyId">The company id</param>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Venue>>>> FetchCompanyVenues(int companyId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Venue>(@"SELECT * FROM ""Venue"" WHERE companyId = @CompanyId ORDER BY venueName ASC LIMIT @Limit OFFSET @Offset", new { CompanyId = companyId, Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Venue>>.None);
                }

                return Result.Ok(Maybe<List<Venue>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Venue>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Venues for a given company id
        /// </summary>
        /// <param name="companyId">The company id</param>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Venue>>>> FetchCountedCompanyVenues(int companyId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                using var obj = await con.QueryMultipleAsync(@"SELECT COUNT(*) FROM ""Venue"" WHERE companyId = @CompanyId; SELECT * FROM ""Venue"" WHERE companyId = @CompanyId ORDER BY venueName ASC LIMIT @Limit OFFSET @Offset", new { CompanyId = companyId, Limit = count, Offset = page * count }).ConfigureAwait(false);
                var totalCount = obj.Read<int>().Single();
                var data = obj.Read<Venue>().ToList();

                var paginatedData = new PaginatedResult<Venue> {
                    Data = data ?? new List<Venue>(),
                    Count = totalCount
                };

                return Result.Ok(Maybe<PaginatedResult<Venue>>.From(paginatedData));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<Venue>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Venues
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Venue>>>> FetchCountedQueriedVenues(string query, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var lookup = new Dictionary<int, Venue>();
                var totalCount = await con.QuerySingleAsync<int>(@"SELECT COUNT(*) FROM ""Venue"" where @Query <-> ""Venue"".venuename > 0.3 or @Query <-> ""Venue"".venueaddress > 0.3;", new { Query = query }).ConfigureAwait(false);
                var data = (await con.QueryAsync<Venue, Tag, Image, Venue>(@"
                    select ""Venue"".*, ""Tag"".*, ""Image"".* from ""Venue""
                    left outer join ""VenueTag""
                    on ""VenueTag"".venueId = ""Venue"".venueId
                    left outer join ""Tag""
                    on ""Tag"".tagId = ""VenueTag"".tagId
                    left outer join ""Image""
                    on ""Image"".imageid = ""Venue"".imageid
                    where @Query <-> ""Venue"".venuename > 0.3
                    or @Query <-> ""Venue"".venueaddress > 0.3
                    group by ""Venue"".venueid, ""Image"".imagekey, ""VenueTag"".venuetagid, ""Tag"".tagid, ""Image"".imageid
                    order by @Query <-> ""Venue"".venuename, @Query <-> ""Venue"".venueaddress
                    LIMIT @Limit OFFSET @Offset",
                  map: (Venue venue, Tag tag, Image image) =>
                  {
                      Venue curVenue;
                      if (!lookup.TryGetValue(venue.VenueId, out curVenue))
                      {
                          lookup.Add(venue.VenueId, curVenue = venue);
                      }

                      if (venue.Tags == null)
                      {
                          venue.Tags = new List<string>();
                      }

                      if (tag != null)
                      {
                          venue.Tags.Add(tag.TagName);
                      }

                      if (image != null)
                      {
                          venue.ImageUrl = $"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{curVenue.CompanyId}__{image.ImageKey}__img";
                      }

                      return curVenue;
                  },
                  splitOn: "tagId,imageId",
                  param: new { Limit = count, Offset = page * count, Query = query }
                ).ConfigureAwait(false)).GroupBy(x => x.VenueId).Select(y => y.First()).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<PaginatedResult<Venue>>.None);
                }

                return Result.Ok(Maybe<PaginatedResult<Venue>>.From(new PaginatedResult<Venue> {
                    Count = totalCount,
                    Data = data
                }));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<Venue>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Venues
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Venue>>>> FetchCountedQueriedCompanyVenues(int companyId, string query, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var lookup = new Dictionary<int, Venue>();
                var totalCount = await con.QuerySingleAsync<int>(@"SELECT COUNT(*) FROM ""Venue"" where companyId = @CompanyId AND (@Query <-> ""Venue"".venuename > 0.3 or @Query <-> ""Venue"".venueaddress > 0.3);", new { CompanyId = companyId, Query = query }).ConfigureAwait(false);
                var data = (await con.QueryAsync<Venue, Tag, Image, Venue>(@"
                    select ""Venue"".*, ""Tag"".*, ""Image"".* from ""Venue""
                    left outer join ""VenueTag""
                    on ""VenueTag"".venueId = ""Venue"".venueId
                    left outer join ""Tag""
                    on ""Tag"".tagId = ""VenueTag"".tagId
                    left outer join ""Image""
                    on ""Image"".imageid = ""Venue"".imageid
                    where ""Venue"".companyId = @CompanyId
                    and (@Query <-> ""Venue"".venuename > 0.3
                    or @Query <-> ""Venue"".venueaddress > 0.3)
                    group by ""Venue"".venueid, ""Image"".imagekey, ""VenueTag"".venuetagid, ""Tag"".tagid, ""Image"".imageid
                    order by @Query <-> ""Venue"".venuename, @Query <-> ""Venue"".venueaddress
                    LIMIT @Limit OFFSET @Offset",
                  map: (Venue venue, Tag tag, Image image) =>
                  {
                      Venue curVenue;
                      if (!lookup.TryGetValue(venue.VenueId, out curVenue))
                      {
                          lookup.Add(venue.VenueId, curVenue = venue);
                      }

                      if (venue.Tags == null)
                      {
                          venue.Tags = new List<string>();
                      }

                      if (tag != null)
                      {
                          venue.Tags.Add(tag.TagName);
                      }

                      if (image != null)
                      {
                          venue.ImageUrl = $"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{curVenue.CompanyId}__{image.ImageKey}__img";
                      }

                      return curVenue;
                  },
                  splitOn: "tagId,imageId",
                  param: new { CompanyId = companyId, Limit = count, Offset = page * count, Query = query }
                ).ConfigureAwait(false)).GroupBy(x => x.VenueId).Select(y => y.First()).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<PaginatedResult<Venue>>.None);
                }

                return Result.Ok(Maybe<PaginatedResult<Venue>>.From(new PaginatedResult<Venue>
                {
                    Count = totalCount,
                    Data = data
                }));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<Venue>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single Venue
        /// </summary>
        /// <param name="resourceId">The id of the Venue you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<Venue>>> FetchFullVenue(int resourceId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var lookup = new Dictionary<int, Venue>();
                var data = (await con.QueryAsync<Venue, Tag, Image, VenueOpeningTime, Venue>(@"
            SELECT ""Venue"".*, ""Tag"".*, ""Image"".*, ""VenueOpeningTime"".* FROM ""Venue""
            left outer join ""VenueTag""
            on ""VenueTag"".venueId = ""Venue"".venueId
            left outer join ""Tag""
            on ""Tag"".tagId = ""VenueTag"".tagId
            left outer join ""Image""
            on ""Image"".imageid = ""Venue"".imageid
            left join ""VenueOpeningTime""
            on (
                ""VenueOpeningTime"".venueId = ""Venue"".venueId and 
                (
                    (""VenueOpeningTime"".starttime <= @CurrentTime and ""VenueOpeningTime"".dayofweek = @DayOfWeek) 
                    or 
                    (""VenueOpeningTime"".endtime < ""VenueOpeningTime"".starttime and ""VenueOpeningTime"".endtime > @CurrentTime and ""VenueOpeningTime"".dayofweek = @DayOfWeek)
                )
            )
            WHERE ""Venue"".venueId = @VenueId
            group by ""Venue"".venueid, ""Image"".imagekey, ""VenueTag"".venuetagid, ""Tag"".tagid, ""Image"".imageid, ""VenueOpeningTime"".venueopeningtimeid",
                  map: (Venue venue, Tag tag, Image image, VenueOpeningTime venueOpeningTime) =>
                  {
                      Venue curVenue;
                      if (!lookup.TryGetValue(venue.VenueId, out curVenue))
                      {
                          lookup.Add(venue.VenueId, curVenue = venue);
                      }

                      if (venue.Tags == null)
                      {
                          venue.Tags = new List<string>();
                      }

                      if (tag != null)
                      {
                          venue.Tags.Add(tag.TagName);
                      }

                      if (image != null)
                      {
                          venue.ImageUrl = $"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{curVenue.CompanyId}__{image.ImageKey}__img";
                      }

                      if (venueOpeningTime != null)
                      {
                          venue.IsOpen = true;
                      }

                      return curVenue;
                  },
                  splitOn: "tagId,imageId,venueOpeningTimeId",
                  param: new { VenueId = resourceId, DayOfWeek = (int)DateTime.UtcNow.DayOfWeek, CurrentTime = TimeSpanHelper.LondonNow() }).ConfigureAwait(false)).FirstOrDefault();

                if (data == null)
                {
                    return Result.Ok(Maybe<Venue>.None);
                }

                return Result.Ok(Maybe<Venue>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<Venue>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Venues
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Venue>>>> FetchNearbyVenues(double lat, double lon, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var lookup = new Dictionary<int, Venue>();
                var data = (await con.QueryAsync<Venue, Tag, Image, VenueOpeningTime, Venue>(@"
            SELECT ""Venue"".*, ""Tag"".*, ""Image"".*, ""VenueOpeningTime"".* FROM ""Venue""
            left outer join ""VenueTag""
            on ""VenueTag"".venueId = ""Venue"".venueId
            left outer join ""Tag""
            on ""Tag"".tagId = ""VenueTag"".tagId
            left outer join ""Image""
            on ""Image"".imageid = ""Venue"".imageid
            left join ""VenueOpeningTime""
            on (
                ""VenueOpeningTime"".venueId = ""Venue"".venueId and 
                (
                    (""VenueOpeningTime"".starttime <= @CurrentTime and ""VenueOpeningTime"".dayofweek = @DayOfWeek) 
                    or 
                    (""VenueOpeningTime"".endtime < ""VenueOpeningTime"".starttime and ""VenueOpeningTime"".endtime > @CurrentTime and ""VenueOpeningTime"".dayofweek = @DayOfWeek)
                )
            )
            WHERE ST_DWithin(ST_MakePoint(@Lat, @Lon), venueCoordinate, 300)
            AND ""Venue"".verificationstatus = 1
            group by ""Venue"".venueid, ""Image"".imagekey, ""VenueTag"".venuetagid, ""Tag"".tagid, ""Image"".imageid, ""VenueOpeningTime"".venueopeningtimeid
            ORDER BY ST_Distance (ST_MakePoint(@Lat, @Lon), venueCoordinate) ASC
            LIMIT @Limit OFFSET @Offset",
                  map: (Venue venue, Tag tag, Image image, VenueOpeningTime venueOpeningTime) =>
                  {
                      Venue curVenue;
                      if (!lookup.TryGetValue(venue.VenueId, out curVenue))
                      {
                          lookup.Add(venue.VenueId, curVenue = venue);
                      }

                      if (venue.Tags == null)
                      {
                          venue.Tags = new List<string>();
                      }

                      if (tag != null)
                      {
                          venue.Tags.Add(tag.TagName);
                      }

                      if (image != null)
                      {
                          venue.ImageUrl = $"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{curVenue.CompanyId}__{image.ImageKey}__img";
                      }

                      if (venueOpeningTime != null)
                      {
                          venue.IsOpen = true;
                      }

                      return curVenue;
                  },
                  splitOn: "tagId,imageId,venueOpeningTimeId",
                  param: new { Limit = count, Offset = page * count, Lat = lat, Lon = lon, DayOfWeek = (int) DateTime.UtcNow.DayOfWeek, CurrentTime = TimeSpanHelper.LondonNow() }
                ).ConfigureAwait(false)).GroupBy(x => x.VenueId).Select(y => y.First()).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Venue>>.None);
                }

                return Result.Ok(Maybe<List<Venue>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Venue>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Venues
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Venue>>>> FetchQueriedVenues(string query, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var lookup = new Dictionary<int, Venue>();
                var data = (await con.QueryAsync<Venue, Tag, Image, VenueOpeningTime, Venue>(@"
            select ""Venue"".*, ""Tag"".*, ""Image"".*, ""VenueOpeningTime"".* from ""Venue""
            left outer join ""VenueTag""
            on ""VenueTag"".venueId = ""Venue"".venueId
            left outer join ""Tag""
            on ""Tag"".tagId = ""VenueTag"".tagId
            left outer join ""Image""
            on ""Image"".imageid = ""Venue"".imageid
            left join ""VenueOpeningTime""
            on (
                ""VenueOpeningTime"".venueId = ""Venue"".venueId and 
                (
                    (""VenueOpeningTime"".starttime <= @CurrentTime and ""VenueOpeningTime"".dayofweek = @DayOfWeek) 
                    or 
                    (""VenueOpeningTime"".endtime < ""VenueOpeningTime"".starttime and ""VenueOpeningTime"".endtime > @CurrentTime and ""VenueOpeningTime"".dayofweek = @DayOfWeek)
                )
            )
            where @Query <-> ""Venue"".venuename > 0.3
            or @Query <-> ""Venue"".venueaddress > 0.3
            group by ""Venue"".venueid, ""Image"".imagekey, ""VenueTag"".venuetagid, ""Tag"".tagid, ""Image"".imageid, ""VenueOpeningTime"".venueopeningtimeid
            order by @Query <-> ""Venue"".venuename, @Query <-> ""Venue"".venueaddress
            LIMIT @Limit OFFSET @Offset",
                  map: (Venue venue, Tag tag, Image image, VenueOpeningTime venueOpeningTime) =>
                  {
                      Venue curVenue;
                      if (!lookup.TryGetValue(venue.VenueId, out curVenue))
                      {
                          lookup.Add(venue.VenueId, curVenue = venue);
                      }

                      if (venue.Tags == null)
                      {
                          venue.Tags = new List<string>();
                      }

                      if (tag != null)
                      {
                          venue.Tags.Add(tag.TagName);
                      }

                      if (image != null)
                      {
                          venue.ImageUrl = $"https://s3-eu-west-1.amazonaws.com/{settings.Connection.S3BucketName}/images/{curVenue.CompanyId}__{image.ImageKey}__img";
                      }

                      if (venueOpeningTime != null)
                      {
                          venue.IsOpen = true;
                      }

                      return curVenue;
                  },
                  splitOn: "tagId,imageId,venueOpeningTimeId",
                  param: new { Limit = count, Offset = page * count, Query = query, DayOfWeek = (int)DateTime.UtcNow.DayOfWeek, CurrentTime = TimeSpanHelper.LondonNow() }
                ).ConfigureAwait(false)).GroupBy(x => x.VenueId).Select(y => y.First()).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Venue>>.None);
                }

                return Result.Ok(Maybe<List<Venue>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Venue>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Venues
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Venue>>>> FetchSimpleQueriedVenues(string query, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var lookup = new Dictionary<int, Venue>();
                var data = (await con.QueryAsync<Venue>(@"
            select * from ""Venue""
            where @Query <-> venuename > 0.3
            order by @Query <-> venuename
            LIMIT @Limit OFFSET @Offset",
                  new { Limit = count, Offset = page * count, Query = query }
                ).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Venue>>.None);
                }

                return Result.Ok(Maybe<List<Venue>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Venue>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Venues
        /// </summary>
        /// <param name="companyId">The venue's company id, or null to return all venues</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<VenueItem>>>> FetchVenueItems(int? companyId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var query = companyId == null
                    ? @"SELECT companyId, venueId, venueName FROM ""Venue"" ORDER BY venueName ASC"
                    : @"SELECT companyId, venueId, venueName FROM ""Venue"" WHERE companyId = @CompanyId ORDER BY venueName ASC";
                var data = (await con.QueryAsync<VenueItem>(query, new { CompanyId = companyId }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<VenueItem>>.None);
                }

                return Result.Ok(Maybe<List<VenueItem>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<VenueItem>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Inserts a new Venue
        /// </summary>
        /// <param name="newVenue">The Venue to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result<Maybe<int>>> CreateVenueWithCoordinates(Venue newVenue)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<int>(
                    @"INSERT INTO ""Venue""(
                            companyId,
                            venueCode,
                            venueName,
                            venueAddress,
                            venueAddress2,
                            venueAddress3,
                            venueCounty,
                            venuePostCode,
                            venuePhone,
                            venueContact,
                            venueDescription,
                            venueNotes,
                            imageId,
                            venueLatitude,
                            venueLongitude,
                            venueCoordinate
                        ) VALUES (
                            @CompanyId,
                            @VenueCode,
                            @VenueName,
                            @VenueAddress,
                            @VenueAddress2,
                            @VenueAddress3,
                            @VenueCounty,
                            @VenuePostCode,
                            @VenuePhone,
                            @VenueContact,
                            @VenueDescription,
                            @VenueNotes,
                            @ImageId,
                            @VenueLatitude,
                            @VenueLongitude,
                            ST_MakePoint(cast(@VenueLatitude as double precision), cast(@VenueLongitude as double precision))
                        ) RETURNING venueId",
                    newVenue
                ).ConfigureAwait(false)).FirstOrDefault();
                if (data < 1)
                {
                    return Result.Ok(Maybe<int>.None);
                }

                return Result.Ok(Maybe<int>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<int>>(ex.ToString());
            }
        }

        /// <summary>
        /// Updates a venue's coordinates
        /// </summary>
        /// <param name="newVenue">The Venue to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result<bool>> UpdateVenueCoordinates(int venueId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync(
                    @"UPDATE ""Venue"" SET venueCoordinate = ST_MakePoint(cast(venueLatitude as double precision), cast(venueLongitude as double precision)) WHERE venueId = @VenueId AND venueLatitude IS NOT NULL AND venueLongitude IS NOT NULL",
                    new {
                        VenueId = venueId
                    }
                ).ConfigureAwait(false);
                return Result.Ok(true);
            }
            catch (Exception)
            {
                return Result.Ok(false);
            }
        }

        /// <summary>
        /// Fetches multiple Venues for a given company id
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Venue>>>> FetchCountedPendingVerifyVenues(int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                using var obj = await con.QueryMultipleAsync(@"SELECT COUNT(*) FROM ""Venue"" WHERE verificationStatus = 0; SELECT * FROM ""Venue"" WHERE verificationStatus = 0 ORDER BY venueName ASC LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false);
                var totalCount = obj.Read<int>().Single();
                var data = obj.Read<Venue>().ToList();

                var paginatedData = new PaginatedResult<Venue>
                {
                    Data = data ?? new List<Venue>(),
                    Count = totalCount
                };

                return Result.Ok(Maybe<PaginatedResult<Venue>>.From(paginatedData));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<Venue>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Venues for a given company id
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Venue>>>> FetchCountedRejectedVenues(int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                using var obj = await con.QueryMultipleAsync(@"SELECT COUNT(*) FROM ""Venue"" WHERE verificationStatus = 2; SELECT * FROM ""Venue"" WHERE verificationStatus = 2 ORDER BY venueName ASC LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false);
                var totalCount = obj.Read<int>().Single();
                var data = obj.Read<Venue>().ToList();

                var paginatedData = new PaginatedResult<Venue>
                {
                    Data = data ?? new List<Venue>(),
                    Count = totalCount
                };

                return Result.Ok(Maybe<PaginatedResult<Venue>>.From(paginatedData));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<Venue>>>(ex.ToString());
            }
        }
    }
}
