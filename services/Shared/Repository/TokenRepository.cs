using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using Koasta.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Koasta.Shared.Repository
{
    public class TokenRepository : RepositoryBase<ApiToken>
    {
        private readonly ISettings _settings;

        public TokenRepository(ISettings settings)
        {
            _settings = settings;
        }

        public async Task<Result<Maybe<ApiToken>>> GetApiAuthToken(string apiToken)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(_settings.Connection.DatabaseConnectionString);
                var data = await con.QueryAsync<ApiToken>("SELECT apiTokenId, apiTokenValue, description, expiry FROM \"ApiToken\" WHERE apiTokenValue = @token",
                                                            new { token = apiToken }).ConfigureAwait(false);

                if (data?.Any() != true)
                {
                    return Result.Fail<Maybe<ApiToken>>("Not found");
                }

                return Result.Ok(Maybe<ApiToken>.From(data.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<ApiToken>>(ex.ToString());
            }
        }

        public async Task<Result<Maybe<bool>>> InsertApiAuthToken(string apiToken, string description, DateTime expiry)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(_settings.Connection.DatabaseConnectionString);
                var data = await con.QueryAsync<int?>("INSERT INTO \"ApiToken\" (apiTokenValue, description, expiry) VALUES(@token, @description, @expiry)",
                                                          new
                                                          {
                                                              token = apiToken,
                                                              description,
                                                              expiry
                                                          }).ConfigureAwait(false);

                if (data == null)
                {
                    return Result.Ok(Maybe<bool>.None);
                }

                return Result.Ok(Maybe<bool>.From(true));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<bool>>(ex.ToString());
            }
        }

        public async Task<Result<Maybe<bool>>> UpdateApiAuthToken(int id, string apiToken, string description, DateTime expiry)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(_settings.Connection.DatabaseConnectionString);
                var data = await con.QueryAsync<bool>("UPDATE \"ApiToken\" SET apiTokenValue = @token, description = @description, expiry = @expiry WHERE apiTokenId = @id",
                                                          new
                                                          {
                                                              id,
                                                              token = apiToken,
                                                              description,
                                                              expiry
                                                          }).ConfigureAwait(false);

                if (data == null)
                {
                    return Result.Ok(Maybe<bool>.None);
                }

                return Result.Ok(Maybe<bool>.From(true));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<bool>>(ex.ToString());
            }
        }

        public async Task<Result<Maybe<bool>>> RemoveApiAuthToken(int apiTokenId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(_settings.Connection.DatabaseConnectionString);
                var data = await con.QueryAsync<bool>("DELETE FROM \"ApiToken\" WHERE apiTokenId = @apiTokenId",
                                                            new { apiTokenId }).ConfigureAwait(false);

                if (data == null)
                {
                    return Result.Ok(Maybe<bool>.None);
                }

                return Result.Ok(Maybe<bool>.From(true));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<bool>>(ex.ToString());
            }
        }
    }
}
