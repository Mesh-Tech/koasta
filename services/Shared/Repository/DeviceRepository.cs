using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;

namespace Koasta.Shared.Database
{
    /*
    A repository for data access to Device resources.
    */
    public partial class DeviceRepository : RepositoryBase<Device>
    {
        /// <summary>
        /// Fetches multiple Devices for a given user
        /// </summary>
        /// <param name="userId">The user id you're querying against</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Device>>>> FetchDevicesForUser(int userId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Device>("SELECT * FROM \"UserDevice\" WHERE userId = @UserId", new { UserId = userId }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Device>>.None);
                }

                return Result.Ok(Maybe<List<Device>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Device>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes a single User's Device
        /// </summary>
        /// <param name="resourceId">The id of the Device you wish to delete</param>
        /// <param name="userId">The id of the User whose Device you wish to delete</param>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> DropUserDevice(int resourceId, int userId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync("DELETE FROM \"UserDevice\" WHERE deviceId = @ResourceId AND userId = @UserId", new { ResourceId = resourceId, UserId = userId }).ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }
    }
}
