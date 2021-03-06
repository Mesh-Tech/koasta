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
    A repository for data access to FeatureFlag resources.
    */
    [CompilerGeneratedAttribute()]
    public partial class FeatureFlagRepository : RepositoryBase<FeatureFlag>
    {
        private readonly ISettings settings;
        private readonly ILogger logger;

        public FeatureFlagRepository(ISettings settings, ILoggerFactory logger) : base()
        {
            this.settings = settings;
            this.logger = logger.CreateLogger("FeatureFlagRepository");
        }

        /// <summary>
        /// Fetches multiple FeatureFlags
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<FeatureFlag>>>> FetchFeatureFlags(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<FeatureFlag>("SELECT * FROM \"FeatureFlag\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<List<FeatureFlag>>.None);
                    }

                    return Result.Ok(Maybe<List<FeatureFlag>>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<List<FeatureFlag>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple FeatureFlags
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<FeatureFlag>>>> FetchCountedFeatureFlags(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"FeatureFlag\"; SELECT * FROM \"FeatureFlag\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false);
                    var totalCount = obj.Read<int>().Single();
                    var data = obj.Read<FeatureFlag>().ToList();

                    var paginatedData = new PaginatedResult<FeatureFlag> {
                      Data = data ?? new List<FeatureFlag>(),
                      Count = totalCount
                    };

                    return Result.Ok(Maybe<PaginatedResult<FeatureFlag>>.From(paginatedData));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<PaginatedResult<FeatureFlag>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single FeatureFlag
        /// </summary>
        /// <param name="resourceId">The id of the FeatureFlag you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<FeatureFlag>>> FetchFeatureFlag(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<FeatureFlag>("SELECT * FROM \"FeatureFlag\" WHERE flagId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false)).FirstOrDefault();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<FeatureFlag>.None);
                    }

                    return Result.Ok(Maybe<FeatureFlag>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<FeatureFlag>>(ex.ToString());
            }
        }

        /// <summary>
        /// Inserts a new FeatureFlag
        /// </summary>
        /// <param name="newFeatureFlag">The FeatureFlag to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result<Maybe<int>>> CreateFeatureFlag(FeatureFlag newFeatureFlag)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<int>(
                        @"INSERT INTO ""FeatureFlag""(
                            
                            name,
                            description,
                            value
                        ) VALUES (
                            
                            @Name,
                            @Description,
                            @Value
                        ) RETURNING flagId",
                        newFeatureFlag
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
        /// Deletes a single FeatureFlag
        /// </summary>
        /// <param name="resourceId">The id of the FeatureFlag you wish to delete</param>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> DropFeatureFlag(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync("DELETE FROM \"FeatureFlag\" WHERE flagId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false);
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
        /// Replaces a single FeatureFlag with a new full set of values
        /// </summary>
        /// <param name="replacedFeatureFlag">The new data for the FeatureFlag you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> ReplaceFeatureFlag(FeatureFlag replacedFeatureFlag)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync(
                        @"UPDATE ""FeatureFlag""
                        SET
                        flagId = @FlagId,
                        name = @Name,
                        description = @Description,
                        value = @Value
                        WHERE flagId = @FeatureFlagId",
                        replacedFeatureFlag
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
        /// Updates a single FeatureFlag with one or more values
        /// </summary>
        /// <param name="updatedFeatureFlag">The new data for the FeatureFlag you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> UpdateFeatureFlag(FeatureFlagPatch updatedFeatureFlag)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var sqlPatchOperations = new StringBuilder();
                    var obj = updatedFeatureFlag;
                    var operationCount = 0;

                    if (obj.FlagId != null)
                    {
                        sqlPatchOperations.AppendLine(obj.FlagId.Operation == OperationKind.Remove 
                            ? "flagId = NULL,"
                            : "flagId = @FlagId,"
                        );
                        operationCount++;
                    }
                    if (obj.Name != null)
                    {
                        sqlPatchOperations.AppendLine(obj.Name.Operation == OperationKind.Remove 
                            ? "name = NULL,"
                            : "name = @Name,"
                        );
                        operationCount++;
                    }
                    if (obj.Description != null)
                    {
                        sqlPatchOperations.AppendLine(obj.Description.Operation == OperationKind.Remove 
                            ? "description = NULL,"
                            : "description = @Description,"
                        );
                        operationCount++;
                    }
                    if (obj.Value != null)
                    {
                        sqlPatchOperations.AppendLine(obj.Value.Operation == OperationKind.Remove 
                            ? "value = NULL,"
                            : "value = @Value,"
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

                    await con.ExecuteAsync($"UPDATE \"FeatureFlag\" SET {patchOperations} WHERE flagId = @ResourceId", new {
                        ResourceId = obj.ResourceId,
                        FlagId = (int) (obj.FlagId == default ? default : obj.FlagId.Value),
                        Name = (string) (obj.Name == default ? default : obj.Name.Value),
                        Description = (string) (obj.Description == default ? default : obj.Description.Value),
                        Value = (bool) (obj.Value == default ? default : obj.Value.Value)
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
