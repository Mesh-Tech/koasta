using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Koasta.Shared.Database
{
    public partial class EmployeeRoleRepository : RepositoryBase<EmployeeRole>
    {
        /// <summary>
        /// Fetches a single EmployeeRole
        /// </summary>
        /// <param name="name">The name of the EmployeeRole you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<EmployeeRole>>> FetchEmployeeRoleByName(string name)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<EmployeeRole>("SELECT * FROM \"EmployeeRole\" WHERE roleName = @Name", new { Name = name }).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<EmployeeRole>.None);
                }

                return Result.Ok(Maybe<EmployeeRole>.From(data));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<EmployeeRole>>(ex.ToString());
            }
        }

        /// <summary>
        /// Replaces an employee record with the given ID
        /// </summary>
        /// <param name="roleId">The role id for this request</param>
        /// <param name="role">The new role data</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<bool>> ReplaceEmployeeRole(int roleId, EmployeeRole role)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync(
                    @"UPDATE ""EmployeeRole"" 
                    SET 
                    roleName = @RoleName,
                    canWorkWithVenue = @CanWorkWithVenue,
                    canAdministerVenue = @CanAdministerVenue,
                    canWorkWithCompany = @CanWorkWithCompany,
                    canAdministerCompany = @CanAdministerCompany,
                    canAdministerSystem = @CanAdministerSystem,
                    WHERE employeeId = @EmployeeId"
                , new
                {
                    RoleId = roleId,
                    role.RoleName,
                    role.CanWorkWithVenue,
                    role.CanAdministerVenue,
                    role.CanWorkWithCompany,
                    role.CanAdministerCompany,
                    role.CanAdministerSystem
                }).ConfigureAwait(false);
                return Result.Ok(true);
            }
            catch (Exception ex)
            {
                return Result.Fail<bool>(ex.ToString());
            }
        }
    }
}
