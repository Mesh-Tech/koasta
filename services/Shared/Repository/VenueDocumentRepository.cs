using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;
using System.Linq;

namespace Koasta.Shared.Database
{
    /*
    A repository for data access to VenueDocument resources.
    */
    public partial class VenueDocumentRepository : RepositoryBase<VenueDocument>
    {
        /// <summary>
        /// Inserts a new VenueDocument
        /// </summary>
        /// <param name="newVenueDocuments">The VenueDocument to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result> CreateVenueDocuments(List<VenueDocument> newVenueDocuments)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync(
                    @"INSERT INTO ""VenueDocuments""(
                            venueId,
                            documentId
                        ) VALUES (
                            @VenueId,
                            @DocumentId
                        )",
                    newVenueDocuments
                ).ConfigureAwait(false);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple VenueDocuments
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<VenueDocument>>>> FetchVenueVenueDocuments(int venueId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<VenueDocument>("SELECT * FROM \"VenueDocuments\" WHERE venueId = @VenueId LIMIT @Limit OFFSET @Offset", new { VenueId = venueId, Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<VenueDocument>>.None);
                }

                return Result.Ok(Maybe<List<VenueDocument>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<VenueDocument>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Inserts many Venue Tags
        /// </summary>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result> ReplaceVenueDocuments(int venueId, List<VenueDocument> documents)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.OpenAsync().ConfigureAwait(false);
                var transaction = con.BeginTransaction();

                await con.ExecuteAsync(@"DELETE FROM ""VenueDocuments"" WHERE venueId = @VenueId", new { @VenueId = venueId }).ConfigureAwait(false);
                if (documents.Count > 0) {
                    await con.ExecuteAsync(@"INSERT INTO ""VenueDocuments"" (venueId, documentId) VALUES (@VenueId, @DocumentId)", documents).ConfigureAwait(false);
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
