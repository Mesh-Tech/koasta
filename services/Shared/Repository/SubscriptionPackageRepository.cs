using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;

namespace Koasta.Shared.Database
{
    public partial class SubscriptionPackageRepository : RepositoryBase<SubscriptionPackage>
    {
        /// <summary>
        /// Fetches multiple SubscriptionPackages
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<SubscriptionPackage>>>> FetchSubscriptionPackagesFromIds(List<int> packageIds)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<SubscriptionPackage>("SELECT * FROM \"SubscriptionPackage\" WHERE packageId = ANY(@Ids)", new { Ids = packageIds }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<SubscriptionPackage>>.None);
                }

                return Result.Ok(Maybe<List<SubscriptionPackage>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<SubscriptionPackage>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes entries
        /// </summary>
        /// <param name="planId">The id of the plan whose entries you wish to delete</param>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> ReplaceSubscriptionPackageEntriesForPlanId(int planId, List<int> packageIds)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync("DELETE FROM \"SubscriptionPackageEntries\" WHERE planId = @ResourceId", new { ResourceId = planId }).ConfigureAwait(false);
                await con.ExecuteAsync("INSERT INTO \"SubscriptionPackageEntries\" (packageId) VALUES (@PackageIds)", new { PackageIds = packageIds }).ConfigureAwait(false);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }
    }
}
