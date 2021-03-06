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
    A repository for data access to SubscriptionPlan resources.
    */
    [CompilerGeneratedAttribute()]
    public partial class SubscriptionPlanRepository : RepositoryBase<SubscriptionPlan>
    {
        private readonly ISettings settings;
        private readonly ILogger logger;

        public SubscriptionPlanRepository(ISettings settings, ILoggerFactory logger) : base()
        {
            this.settings = settings;
            this.logger = logger.CreateLogger("SubscriptionPlanRepository");
        }

        /// <summary>
        /// Fetches multiple SubscriptionPlans
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<SubscriptionPlan>>>> FetchSubscriptionPlans(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<SubscriptionPlan>("SELECT * FROM \"SubscriptionPlan\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<List<SubscriptionPlan>>.None);
                    }

                    return Result.Ok(Maybe<List<SubscriptionPlan>>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<List<SubscriptionPlan>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple SubscriptionPlans
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<SubscriptionPlan>>>> FetchCountedSubscriptionPlans(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"SubscriptionPlan\"; SELECT * FROM \"SubscriptionPlan\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false);
                    var totalCount = obj.Read<int>().Single();
                    var data = obj.Read<SubscriptionPlan>().ToList();

                    var paginatedData = new PaginatedResult<SubscriptionPlan> {
                      Data = data ?? new List<SubscriptionPlan>(),
                      Count = totalCount
                    };

                    return Result.Ok(Maybe<PaginatedResult<SubscriptionPlan>>.From(paginatedData));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<PaginatedResult<SubscriptionPlan>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single SubscriptionPlan
        /// </summary>
        /// <param name="resourceId">The id of the SubscriptionPlan you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<SubscriptionPlan>>> FetchSubscriptionPlan(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<SubscriptionPlan>("SELECT * FROM \"SubscriptionPlan\" WHERE planId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false)).FirstOrDefault();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<SubscriptionPlan>.None);
                    }

                    return Result.Ok(Maybe<SubscriptionPlan>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<SubscriptionPlan>>(ex.ToString());
            }
        }

        /// <summary>
        /// Inserts a new SubscriptionPlan
        /// </summary>
        /// <param name="newSubscriptionPlan">The SubscriptionPlan to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result<Maybe<int>>> CreateSubscriptionPlan(SubscriptionPlan newSubscriptionPlan)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<int>(
                        @"INSERT INTO ""SubscriptionPlan""(
                            
                            companyId,
                            externalPlanId
                        ) VALUES (
                            
                            @CompanyId,
                            @ExternalPlanId
                        ) RETURNING planId",
                        newSubscriptionPlan
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
        /// Deletes a single SubscriptionPlan
        /// </summary>
        /// <param name="resourceId">The id of the SubscriptionPlan you wish to delete</param>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> DropSubscriptionPlan(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync("DELETE FROM \"SubscriptionPlan\" WHERE planId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false);
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
        /// Replaces a single SubscriptionPlan with a new full set of values
        /// </summary>
        /// <param name="replacedSubscriptionPlan">The new data for the SubscriptionPlan you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> ReplaceSubscriptionPlan(SubscriptionPlan replacedSubscriptionPlan)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync(
                        @"UPDATE ""SubscriptionPlan""
                        SET
                        planId = @PlanId,
                        companyId = @CompanyId,
                        externalPlanId = @ExternalPlanId
                        WHERE planId = @SubscriptionPlanId",
                        replacedSubscriptionPlan
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
        /// Updates a single SubscriptionPlan with one or more values
        /// </summary>
        /// <param name="updatedSubscriptionPlan">The new data for the SubscriptionPlan you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> UpdateSubscriptionPlan(SubscriptionPlanPatch updatedSubscriptionPlan)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var sqlPatchOperations = new StringBuilder();
                    var obj = updatedSubscriptionPlan;
                    var operationCount = 0;

                    if (obj.PlanId != null)
                    {
                        sqlPatchOperations.AppendLine(obj.PlanId.Operation == OperationKind.Remove 
                            ? "planId = NULL,"
                            : "planId = @PlanId,"
                        );
                        operationCount++;
                    }
                    if (obj.CompanyId != null)
                    {
                        sqlPatchOperations.AppendLine(obj.CompanyId.Operation == OperationKind.Remove 
                            ? "companyId = NULL,"
                            : "companyId = @CompanyId,"
                        );
                        operationCount++;
                    }
                    if (obj.ExternalPlanId != null)
                    {
                        sqlPatchOperations.AppendLine(obj.ExternalPlanId.Operation == OperationKind.Remove 
                            ? "externalPlanId = NULL,"
                            : "externalPlanId = @ExternalPlanId,"
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

                    await con.ExecuteAsync($"UPDATE \"SubscriptionPlan\" SET {patchOperations} WHERE planId = @ResourceId", new {
                        ResourceId = obj.ResourceId,
                        PlanId = (int) (obj.PlanId == default ? default : obj.PlanId.Value),
                        CompanyId = (int) (obj.CompanyId == default ? default : obj.CompanyId.Value),
                        ExternalPlanId = (string) (obj.ExternalPlanId == default ? default : obj.ExternalPlanId.Value)
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
