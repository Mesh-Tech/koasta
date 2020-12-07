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
    A repository for data access to EmployeeRole resources.
    */
    [CompilerGeneratedAttribute()]
    public partial class EmployeeRoleRepository : RepositoryBase<EmployeeRole>
    {
        private readonly ISettings settings;
        private readonly ILogger logger;

        public EmployeeRoleRepository(ISettings settings, ILoggerFactory logger) : base()
        {
            this.settings = settings;
            this.logger = logger.CreateLogger("EmployeeRoleRepository");
        }

        /// <summary>
        /// Fetches multiple EmployeeRoles
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<EmployeeRole>>>> FetchEmployeeRoles(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<EmployeeRole>("SELECT * FROM \"EmployeeRole\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<List<EmployeeRole>>.None);
                    }

                    return Result.Ok(Maybe<List<EmployeeRole>>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<List<EmployeeRole>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple EmployeeRoles
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<EmployeeRole>>>> FetchCountedEmployeeRoles(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"EmployeeRole\"; SELECT * FROM \"EmployeeRole\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false);
                    var totalCount = obj.Read<int>().Single();
                    var data = obj.Read<EmployeeRole>().ToList();

                    var paginatedData = new PaginatedResult<EmployeeRole> {
                      Data = data ?? new List<EmployeeRole>(),
                      Count = totalCount
                    };

                    return Result.Ok(Maybe<PaginatedResult<EmployeeRole>>.From(paginatedData));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<PaginatedResult<EmployeeRole>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single EmployeeRole
        /// </summary>
        /// <param name="resourceId">The id of the EmployeeRole you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<EmployeeRole>>> FetchEmployeeRole(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<EmployeeRole>("SELECT * FROM \"EmployeeRole\" WHERE roleId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false)).FirstOrDefault();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<EmployeeRole>.None);
                    }

                    return Result.Ok(Maybe<EmployeeRole>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<EmployeeRole>>(ex.ToString());
            }
        }

        /// <summary>
        /// Inserts a new EmployeeRole
        /// </summary>
        /// <param name="newEmployeeRole">The EmployeeRole to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result<Maybe<int>>> CreateEmployeeRole(EmployeeRole newEmployeeRole)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<int>(
                        @"INSERT INTO ""EmployeeRole""(
                            
                            roleName,
                            canWorkWithVenue,
                            canAdministerVenue,
                            canWorkWithCompany,
                            canAdministerCompany,
                            canAdministerSystem
                        ) VALUES (
                            
                            @RoleName,
                            @CanWorkWithVenue,
                            @CanAdministerVenue,
                            @CanWorkWithCompany,
                            @CanAdministerCompany,
                            @CanAdministerSystem
                        ) RETURNING roleId",
                        newEmployeeRole
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
        /// Deletes a single EmployeeRole
        /// </summary>
        /// <param name="resourceId">The id of the EmployeeRole you wish to delete</param>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> DropEmployeeRole(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync("DELETE FROM \"EmployeeRole\" WHERE roleId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false);
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
        /// Replaces a single EmployeeRole with a new full set of values
        /// </summary>
        /// <param name="replacedEmployeeRole">The new data for the EmployeeRole you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> ReplaceEmployeeRole(EmployeeRole replacedEmployeeRole)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync(
                        @"UPDATE ""EmployeeRole""
                        SET
                        roleId = @RoleId,
                        roleName = @RoleName,
                        canWorkWithVenue = @CanWorkWithVenue,
                        canAdministerVenue = @CanAdministerVenue,
                        canWorkWithCompany = @CanWorkWithCompany,
                        canAdministerCompany = @CanAdministerCompany,
                        canAdministerSystem = @CanAdministerSystem
                        WHERE roleId = @EmployeeRoleId",
                        replacedEmployeeRole
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
        /// Updates a single EmployeeRole with one or more values
        /// </summary>
        /// <param name="updatedEmployeeRole">The new data for the EmployeeRole you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> UpdateEmployeeRole(EmployeeRolePatch updatedEmployeeRole)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var sqlPatchOperations = new StringBuilder();
                    var obj = updatedEmployeeRole;
                    var operationCount = 0;

                    if (obj.RoleId != null)
                    {
                        sqlPatchOperations.AppendLine(obj.RoleId.Operation == OperationKind.Remove 
                            ? "roleId = NULL,"
                            : "roleId = @RoleId,"
                        );
                        operationCount++;
                    }
                    if (obj.RoleName != null)
                    {
                        sqlPatchOperations.AppendLine(obj.RoleName.Operation == OperationKind.Remove 
                            ? "roleName = NULL,"
                            : "roleName = @RoleName,"
                        );
                        operationCount++;
                    }
                    if (obj.CanWorkWithVenue != null)
                    {
                        sqlPatchOperations.AppendLine(obj.CanWorkWithVenue.Operation == OperationKind.Remove 
                            ? "canWorkWithVenue = NULL,"
                            : "canWorkWithVenue = @CanWorkWithVenue,"
                        );
                        operationCount++;
                    }
                    if (obj.CanAdministerVenue != null)
                    {
                        sqlPatchOperations.AppendLine(obj.CanAdministerVenue.Operation == OperationKind.Remove 
                            ? "canAdministerVenue = NULL,"
                            : "canAdministerVenue = @CanAdministerVenue,"
                        );
                        operationCount++;
                    }
                    if (obj.CanWorkWithCompany != null)
                    {
                        sqlPatchOperations.AppendLine(obj.CanWorkWithCompany.Operation == OperationKind.Remove 
                            ? "canWorkWithCompany = NULL,"
                            : "canWorkWithCompany = @CanWorkWithCompany,"
                        );
                        operationCount++;
                    }
                    if (obj.CanAdministerCompany != null)
                    {
                        sqlPatchOperations.AppendLine(obj.CanAdministerCompany.Operation == OperationKind.Remove 
                            ? "canAdministerCompany = NULL,"
                            : "canAdministerCompany = @CanAdministerCompany,"
                        );
                        operationCount++;
                    }
                    if (obj.CanAdministerSystem != null)
                    {
                        sqlPatchOperations.AppendLine(obj.CanAdministerSystem.Operation == OperationKind.Remove 
                            ? "canAdministerSystem = NULL,"
                            : "canAdministerSystem = @CanAdministerSystem,"
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

                    await con.ExecuteAsync($"UPDATE \"EmployeeRole\" SET {patchOperations} WHERE roleId = @ResourceId", new {
                        ResourceId = obj.ResourceId,
                        RoleId = (int) (obj.RoleId == default ? default : obj.RoleId.Value),
                        RoleName = (string) (obj.RoleName == default ? default : obj.RoleName.Value),
                        CanWorkWithVenue = (bool) (obj.CanWorkWithVenue == default ? default : obj.CanWorkWithVenue.Value),
                        CanAdministerVenue = (bool) (obj.CanAdministerVenue == default ? default : obj.CanAdministerVenue.Value),
                        CanWorkWithCompany = (bool) (obj.CanWorkWithCompany == default ? default : obj.CanWorkWithCompany.Value),
                        CanAdministerCompany = (bool) (obj.CanAdministerCompany == default ? default : obj.CanAdministerCompany.Value),
                        CanAdministerSystem = (bool) (obj.CanAdministerSystem == default ? default : obj.CanAdministerSystem.Value)
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
