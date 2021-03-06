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
    A repository for data access to Document resources.
    */
    [CompilerGeneratedAttribute()]
    public partial class DocumentRepository : RepositoryBase<Document>
    {
        private readonly ISettings settings;
        private readonly ILogger logger;

        public DocumentRepository(ISettings settings, ILoggerFactory logger) : base()
        {
            this.settings = settings;
            this.logger = logger.CreateLogger("DocumentRepository");
        }

        /// <summary>
        /// Fetches multiple Documents
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Document>>>> FetchDocuments(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<Document>("SELECT * FROM \"Document\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<List<Document>>.None);
                    }

                    return Result.Ok(Maybe<List<Document>>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<List<Document>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Documents
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Document>>>> FetchCountedDocuments(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"Document\"; SELECT * FROM \"Document\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false);
                    var totalCount = obj.Read<int>().Single();
                    var data = obj.Read<Document>().ToList();

                    var paginatedData = new PaginatedResult<Document> {
                      Data = data ?? new List<Document>(),
                      Count = totalCount
                    };

                    return Result.Ok(Maybe<PaginatedResult<Document>>.From(paginatedData));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<PaginatedResult<Document>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single Document
        /// </summary>
        /// <param name="resourceId">The id of the Document you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<Document>>> FetchDocument(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<Document>("SELECT * FROM \"Document\" WHERE documentId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false)).FirstOrDefault();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<Document>.None);
                    }

                    return Result.Ok(Maybe<Document>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<Document>>(ex.ToString());
            }
        }

        /// <summary>
        /// Inserts a new Document
        /// </summary>
        /// <param name="newDocument">The Document to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result<Maybe<int>>> CreateDocument(Document newDocument)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<int>(
                        @"INSERT INTO ""Document""(
                            companyId,
                            documentKey,
                            documentTitle,
                            documentDescription
                        ) VALUES (
                            @CompanyId,
                            @DocumentKey,
                            @DocumentTitle,
                            @DocumentDescription
                        ) RETURNING documentId",
                        newDocument
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
        /// Deletes a single Document
        /// </summary>
        /// <param name="resourceId">The id of the Document you wish to delete</param>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> DropDocument(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync("DELETE FROM \"Document\" WHERE documentId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false);
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
        /// Replaces a single Document with a new full set of values
        /// </summary>
        /// <param name="replacedDocument">The new data for the Document you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> ReplaceDocument(Document replacedDocument)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync(
                        @"UPDATE ""Document""
                        SET
                        companyId = @CompanyId,
                        documentKey = @DocumentKey,
                        documentTitle = @DocumentTitle,
                        documentDescription = @DocumentDescription
                        WHERE documentId = @DocumentId",
                        replacedDocument
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
        /// Updates a single Document with one or more values
        /// </summary>
        /// <param name="updatedDocument">The new data for the Document you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> UpdateDocument(DocumentPatch updatedDocument)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var sqlPatchOperations = new StringBuilder();
                    var obj = updatedDocument;
                    var operationCount = 0;

                    if (obj.CompanyId != null)
                    {
                        sqlPatchOperations.AppendLine(obj.CompanyId.Operation == OperationKind.Remove 
                            ? "companyId = NULL,"
                            : "companyId = @CompanyId,"
                        );
                        operationCount++;
                    }
                    if (obj.DocumentKey != null)
                    {
                        sqlPatchOperations.AppendLine(obj.DocumentKey.Operation == OperationKind.Remove 
                            ? "documentKey = NULL,"
                            : "documentKey = @DocumentKey,"
                        );
                        operationCount++;
                    }
                    if (obj.DocumentTitle != null)
                    {
                        sqlPatchOperations.AppendLine(obj.DocumentTitle.Operation == OperationKind.Remove 
                            ? "documentTitle = NULL,"
                            : "documentTitle = @DocumentTitle,"
                        );
                        operationCount++;
                    }
                    if (obj.DocumentDescription != null)
                    {
                        sqlPatchOperations.AppendLine(obj.DocumentDescription.Operation == OperationKind.Remove 
                            ? "documentDescription = NULL,"
                            : "documentDescription = @DocumentDescription,"
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

                    await con.ExecuteAsync($"UPDATE \"Document\" SET {patchOperations} WHERE documentId = @ResourceId", new {
                        ResourceId = obj.ResourceId,
                        CompanyId = (int) (obj.CompanyId == default ? default : obj.CompanyId.Value),
                        DocumentKey = (string) (obj.DocumentKey == default ? default : obj.DocumentKey.Value),
                        DocumentTitle = (string) (obj.DocumentTitle == default ? default : obj.DocumentTitle.Value),
                        DocumentDescription = (string) (obj.DocumentDescription == default ? default : obj.DocumentDescription.Value)
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
