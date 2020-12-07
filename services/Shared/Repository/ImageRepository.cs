using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;
using Koasta.Shared.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Koasta.Shared.Database
{
    public partial class ImageRepository : RepositoryBase<Image>
    {
        /// <summary>
        /// Fetches multiple Images
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Image>>>> FetchCompanyImages(int companyId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Image>("SELECT * FROM \"Image\" WHERE companyId = @CompanyId LIMIT @Limit OFFSET @Offset", new { CompanyId = companyId, Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Image>>.None);
                }

                return Result.Ok(Maybe<List<Image>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Image>>>(ex.ToString());
            }
        }

        /// <summary>
         /// Fetches multiple Images
         /// </summary>
         /// <param name="page">The current page number</param>
         /// <param name="count">The page size</param>
         /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Image>>>> FetchCountedCompanyImages(int companyId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"Image\" WHERE companyId = @CompanyId; SELECT * FROM \"Image\" WHERE companyId = @CompanyId LIMIT @Limit OFFSET @Offset", new { CompanyId = companyId, Limit = count, Offset = page * count }).ConfigureAwait(false);
                var totalCount = obj.Read<int>().Single();
                var data = obj.Read<Image>().ToList();

                var paginatedData = new PaginatedResult<Image>
                {
                    Data = data ?? new List<Image>(),
                    Count = totalCount
                };

                return Result.Ok(Maybe<PaginatedResult<Image>>.From(paginatedData));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<Image>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single Image
        /// </summary>
        /// <param name="resourceId">The id of the Image you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<Image>>> FetchCompanyImage(int companyId, int resourceId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Image>("SELECT * FROM \"Image\" WHERE imageId = @ResourceId AND companyId = @CompanyId", new { ResourceId = resourceId, CompanyId = companyId }).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<Image>.None);
                }

                return Result.Ok(Maybe<Image>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<Image>>(ex.ToString());
            }
        }

        /// <summary>
        /// Updates a single Image's title
        /// </summary>
        /// <param name="companyId">The id of the company whose Image you wish to update</param>
        /// <param name="resourceId">The id of the Image you wish to update</param>
        /// <param name="imageTitle">The new title for this Image</param>
        /// <returns>Returns a result</returns>
        public async Task<Result> UpdateCompanyImage(int companyId, int resourceId, string imageTitle)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync("UPDATE \"Image\" SET imageTitle = @ImageTitle WHERE imageId = @ResourceId AND companyId = @CompanyId", new { ImageTitle = imageTitle, ResourceId = resourceId, CompanyId = companyId }).ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }
    }
}
