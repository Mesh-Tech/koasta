using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Configuration;
using Koasta.Shared.Models;
using Koasta.Shared.PatchModels;
using Koasta.Shared.Types;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

// WARNING: This file is auto-generated from {repository root}/scripts/csport/generate-repositories.js.
//          Do not edit this file directly, as changes will be replaced!

namespace Koasta.Shared.Database
{
    /*
    A repository for data access to VenueDocument resources.
    */
    [CompilerGeneratedAttribute()]
    public partial class VenueDocumentRepository : RepositoryBase<VenueDocument>
    {
        private readonly ISettings settings;
        private readonly ILogger logger;

        public VenueDocumentRepository(ISettings settings, ILoggerFactory logger) : base()
        {
            this.settings = settings;
            this.logger = logger.CreateLogger("VenueDocumentRepository");
        }

        /// <summary>
        /// Fetches multiple VenueDocuments
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<VenueDocument>>>> FetchVenueDocuments(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<VenueDocument>("SELECT * FROM \"VenueDocuments\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<List<VenueDocument>>.None);
                    }

                    return Result.Ok(Maybe<List<VenueDocument>>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<List<VenueDocument>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple VenueDocuments
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<VenueDocument>>>> FetchCountedVenueDocuments(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"VenueDocuments\"; SELECT * FROM \"VenueDocuments\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false);
                    var totalCount = obj.Read<int>().Single();
                    var data = obj.Read<VenueDocument>().ToList();

                    var paginatedData = new PaginatedResult<VenueDocument> {
                      Data = data ?? new List<VenueDocument>(),
                      Count = totalCount
                    };

                    return Result.Ok(Maybe<PaginatedResult<VenueDocument>>.From(paginatedData));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<PaginatedResult<VenueDocument>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single VenueDocument
        /// </summary>
        /// <param name="resourceId">The id of the VenueDocument you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<VenueDocument>>> FetchVenueDocument(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<VenueDocument>("SELECT * FROM \"VenueDocuments\" WHERE venueId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false)).FirstOrDefault();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<VenueDocument>.None);
                    }

                    return Result.Ok(Maybe<VenueDocument>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<VenueDocument>>(ex.ToString());
            }
        }

        /// <summary>
        /// Inserts a new VenueDocument
        /// </summary>
        /// <param name="newVenueDocument">The VenueDocument to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result<Maybe<int>>> CreateVenueDocument(VenueDocument newVenueDocument)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<int>(
                        @"INSERT INTO ""VenueDocuments""(
                            venueId,
                            documentId
                        ) VALUES (
                            @VenueId,
                            @DocumentId
                        ) RETURNING venueId",
                        newVenueDocument
                    ).ConfigureAwait(false)).FirstOrDefault();
                    if (data < 1)
                    {
                        return Result.Ok(Maybe<int>.None);
                    }

                    return Result.Ok(Maybe<int>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<int>>(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes a single VenueDocument
        /// </summary>
        /// <param name="resourceId">The id of the VenueDocument you wish to delete</param>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> DropVenueDocument(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync("DELETE FROM \"VenueDocuments\" WHERE venueId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false);
                    return Result.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Replaces a single VenueDocument with a new full set of values
        /// </summary>
        /// <param name="replacedVenueDocument">The new data for the VenueDocument you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> ReplaceVenueDocument(VenueDocument replacedVenueDocument)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync(
                        @"UPDATE ""VenueDocuments""
                        SET
                        venueId = @VenueId,
                        documentId = @DocumentId
                        WHERE venueId = @VenueDocumentId",
                        replacedVenueDocument
                    ).ConfigureAwait(false);

                    return Result.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Updates a single VenueDocument with one or more values
        /// </summary>
        /// <param name="updatedVenueDocument">The new data for the VenueDocument you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> UpdateVenueDocument(VenueDocumentPatch updatedVenueDocument)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var sqlPatchOperations = new StringBuilder();
                    var obj = updatedVenueDocument;
                    var operationCount = 0;

                    if (obj.VenueId != null)
                    {
                        sqlPatchOperations.AppendLine(obj.VenueId.Operation == OperationKind.Remove 
                            ? "venueId = NULL,"
                            : "venueId = @VenueId,"
                        );
                        operationCount++;
                    }
                    if (obj.DocumentId != null)
                    {
                        sqlPatchOperations.AppendLine(obj.DocumentId.Operation == OperationKind.Remove 
                            ? "documentId = NULL,"
                            : "documentId = @DocumentId,"
                        );
                        operationCount++;
                    }

                    var patchOperations = sqlPatchOperations.ToString();

                    if (operationCount > 0)
                    {
                        // Remove final ", " from StringBuilder to ensure query is valid
                        patchOperations = patchOperations.TrimEnd(System.Environment.NewLine.ToCharArray());
                        patchOperations = patchOperations.TrimEnd(',');
                    }

                    await con.ExecuteAsync($"UPDATE \"VenueDocuments\" SET {patchOperations} WHERE venueId = @ResourceId", new {
                        ResourceId = obj.ResourceId,
                        VenueId = (int) (obj.VenueId == default ? default : obj.VenueId.Value),
                        DocumentId = (int) (obj.DocumentId == default ? default : obj.DocumentId.Value)
                    }).ConfigureAwait(false);

                    return Result.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail(ex.ToString());
            }
        }
    }
}
