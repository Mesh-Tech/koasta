using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;

namespace Koasta.Shared.Database
{
    public partial class EmployeeSessionRepository : RepositoryBase<EmployeeSession>
    {
        /// <summary>
        /// Fetches an employee associated to an employee session auth token
        /// </summary>
        /// <param name="authToken">The session token you're querying against</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<Employee>>> FetchEmployeeSessionAndEmployeeFromToken(string authToken)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Employee>(
                  @"SELECT emp.* FROM ""EmployeeSession"" emps
            INNER JOIN ""Employee"" emp
            ON emps.employeeId = emp.employeeId
            WHERE emps.authToken = @AuthToken
            AND emps.expiry > @DtNow",

                  new { AuthToken = authToken, DtNow = DateTime.UtcNow }
                ).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<Employee>.None);
                }

                return Result.Ok(Maybe<Employee>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<Employee>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches an employee session associated to a session token pair
        /// </summary>
        /// <param name="authToken">The auth token you're querying against</param>
        /// <param name="refreshToken">The refresh token you're querying against</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<EmployeeSession>>> FetchEmployeeSessionFromTokens(string authToken, string refreshToken)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<EmployeeSession>(
                  @"SELECT * FROM ""EmployeeSession""
            WHERE refreshToken = @RefreshToken
            AND authToken = @AuthToken
            AND refreshExpiry > @DtNow",

                  new { RefreshToken = refreshToken, AuthToken = authToken, DtNow = DateTime.UtcNow }
                ).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<EmployeeSession>.None);
                }

                return Result.Ok(Maybe<EmployeeSession>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<EmployeeSession>>(ex.ToString());
            }
        }
    }
}
