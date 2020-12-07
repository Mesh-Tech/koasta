using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;

namespace Koasta.Shared.Database
{
    public partial class UserRepository : RepositoryBase<User>
    {
        /// <summary>
        /// Fetches a user associated to a user session auth token
        /// </summary>
        /// <param name="authToken">The session token you're querying against</param>
        /// <param name="authType">The token type</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<User>>> FetchUserForAuthIdentifier(string authIdentifier, AuthType authType)
        {
            string column;
            switch (authType)
            {
                case AuthType.Apple:
                    column = "appleUserIdentifier";
                    break;
                case AuthType.Facebook:
                    column = "facebookUserIdentifier";
                    break;
                case AuthType.Google:
                    column = "googleUserIdentifier";
                    break;
                default:
                    return Result.Ok(Maybe<User>.From(null));
            }

            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);

                var data = (await con.QueryAsync<User>(
                  $@"SELECT * FROM ""User"" WHERE {column} = @AuthIdentifier",
                  new { AuthIdentifier = authIdentifier }
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
