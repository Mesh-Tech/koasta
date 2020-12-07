using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Koasta.Shared.Models;
using Koasta.Shared.Types;
using System.Linq;

namespace Koasta.Shared.Database
{
    public class MediaRepository : RepositoryBase<AverageOrderValue>
    {
        private readonly ISettings settings;

        public MediaRepository(ISettings settings) : base()
        {
            this.settings = settings;
        }

        /// <summary>
        /// Fetches multiple media entries
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<MediaEntry>>>> FetchCountedMediaEntries(int page, int count)
        {
            try
            {
                const string cquery = @"select count(*) from (
                                            select i.imageid as id
                                            from ""Image"" i
                                            union all
                                            select d.documentid  as id
                                            from ""Document"" d
                                            order by id
                                        ) total";
                const string query = @"select i.imageid as id, i.companyid as companyid, i.imagekey as key, i.imagetitle as title, CAST(1 as int) as mediatype
                                       from ""Image"" i
                                       union all
                                       select d.documentid  as id, d.companyid  as companyid, d.documentkey as key, d.documenttitle as title, CAST(2 as int) as mediatype
                                       from ""Document"" d
                                       order by id
                                       limit 20 
                                       offset 0";

                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                using var obj = await con.QueryMultipleAsync($"{cquery}; {query}", new { Limit = count, Offset = page * count }).ConfigureAwait(false);
                var totalCount = obj.Read<int>().Single();
                var data = obj.Read<MediaEntry>().ToList();

                var paginatedData = new PaginatedResult<MediaEntry>
                {
                    Data = data ?? new List<MediaEntry>(),
                    Count = totalCount
                };

                return Result.Ok(Maybe<PaginatedResult<MediaEntry>>.From(paginatedData));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<MediaEntry>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple media entries
        /// </summary>
        /// <param name="companyId">The company id to query against</param>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<MediaEntry>>>> FetchCountedCompanyMediaEntries(int companyId, int page, int count)
        {
            try
            {
                const string cquery = @"select count(*) from (
                                            select i.imageid as id
                                            from ""Image"" i
                                            where i.companyId = @CompanyId
                                            union all
                                            select d.documentid  as id
                                            from ""Document"" d
                                            where d.companyId = @CompanyId
                                            order by id
                                        ) total";
                const string query = @"select i.imageid as id, i.companyid as companyid, i.imagekey as key, i.imagetitle as title, CAST(1 as int) as mediatype
                                       from ""Image"" i
                                       where i.companyId = @CompanyId
                                       union all
                                       select d.documentid  as id, d.companyid  as companyid, d.documentkey as key, d.documenttitle as title, CAST(2 as int) as mediatype
                                       from ""Document"" d
                                       where d.companyId = @CompanyId
                                       order by id
                                       limit 20 
                                       offset 0";

                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                using var obj = await con.QueryMultipleAsync($"{cquery}; {query}", new { Limit = count, Offset = page * count, CompanyId = companyId }).ConfigureAwait(false);
                var totalCount = obj.Read<int>().Single();
                var data = obj.Read<MediaEntry>().ToList();

                var paginatedData = new PaginatedResult<MediaEntry>
                {
                    Data = data ?? new List<MediaEntry>(),
                    Count = totalCount
                };

                return Result.Ok(Maybe<PaginatedResult<MediaEntry>>.From(paginatedData));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<MediaEntry>>>(ex.ToString());
            }
        }
    }
}
