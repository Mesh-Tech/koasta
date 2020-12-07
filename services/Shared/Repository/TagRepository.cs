using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Koasta.Shared.Database
{
    class VenueTag {
        [Column("venueId")]
        public int VenueId { get; set; }
        [Column("tagId")]
        public int TagId { get; set; }
    }

    public partial class TagRepository : RepositoryBase<Tag>
    {
        /// <summary>
        /// Fetches multiple Tags
        /// </summary>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Tag>>>> FetchVenueTags(int venueId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Tag>(@"SELECT * FROM ""Tag"" AS t
                                       INNER JOIN ""VenueTag"" vt
                                       ON t.tagId = vt.tagId
                                       WHERE vt.venueId = @VenueId", new { VenueId = venueId }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Tag>>.None);
                }

                return Result.Ok(Maybe<List<Tag>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Tag>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Inserts many Venue Tags
        /// </summary>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result> CreateVenueTags(int venueId, List<int> tagIds)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.OpenAsync().ConfigureAwait(false);
                var transaction = con.BeginTransaction();

                await Task.WhenAll(tagIds.Select(async _ =>
                {
                    return await con.ExecuteAsync(
                      @"INSERT INTO ""VenueTag""(
                venueId, tagId
              ) VALUES (
                @VenueId, @TagId
              )",
                      tagIds.Select(t => new { VenueId = venueId, TagId = t }).ToList()
                    ).ConfigureAwait(false);
                }).ToArray()).ConfigureAwait(false);

                await transaction.CommitAsync().ConfigureAwait(false);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes a single Venue Tag
        /// </summary>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> DropVenueTags(int venueId, List<int> tagIds)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync("DELETE FROM \"VenueTag\" WHERE tagId = ANY(@Tags) AND venueId = @VenueId", new { Tags = tagIds, VenueId = venueId }).ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Replaces a venue's set of tags with a new set
        /// </summary>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> ReplaceVenueTags(int venueId, List<string> tagNames)
        {
            if (tagNames.Count == 0)
            {
                return Result.Ok();
            }

            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.OpenAsync().ConfigureAwait(false);
                var tran = await con.BeginTransactionAsync().ConfigureAwait(false);

                var tags = await con.QueryAsync<Tag>("SELECT * FROM \"Tag\" WHERE tagname = ANY(@TagNames)", new { TagNames = tagNames }).ConfigureAwait(false);
                await con.ExecuteAsync("DELETE FROM \"VenueTag\" WHERE venueId = @VenueId", new { VenueId = venueId }).ConfigureAwait(false);

                var venueTags = tags.Select(t => new VenueTag { VenueId = venueId, TagId = t.TagId }).ToList();

                await con.ExecuteAsync("INSERT INTO \"VenueTag\" (venueid, tagid) VALUES (@VenueId, @TagId)", venueTags).ConfigureAwait(false);

                try {
                    await tran.CommitAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await tran.RollbackAsync().ConfigureAwait(false);
                    return Result.Fail(ex.ToString());
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }
    }
}
