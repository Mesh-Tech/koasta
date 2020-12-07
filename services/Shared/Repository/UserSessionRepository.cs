using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;

namespace Koasta.Shared.Database
{
    public partial class UserSessionRepository : RepositoryBase<UserSession>
    {
        /// <summary>
        /// Fetches a user associated to a user session auth token
        /// </summary>
        /// <param name="authToken">The session token you're querying against</param>
        /// <param name="authType">The token type</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<User>>> FetchUserSessionAndUserFromToken(string authToken, int authType)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<User>(
                  @"SELECT u.* FROM ""UserSession"" us
            INNER JOIN ""User"" u
            ON us.userId = u.userId
            WHERE us.authToken = @AuthToken
            AND us.authTokenExpiry > @DtNow
            AND us.type = @AuthType",

                  new { AuthToken = authToken, DtNow = DateTime.UtcNow, AuthType = authType }
                ).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<User>.None);
                }

                return Result.Ok(Maybe<User>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<User>>(ex.ToString());
            }
        }
    }
}
