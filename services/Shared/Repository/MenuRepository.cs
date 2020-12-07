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
    public partial class MenuRepository
    {
        /// <summary>
        /// Fetches multiple Menus
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<FullMenu>>>> FetchMenusForVenue(int venueId)
        {
            try
            {
                var fullMenuDetails = new List<FullMenu>();
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var lookup = new Dictionary<int, FullMenu>();
                var data = await con.QueryMultipleAsync(
                  @"SELECT m.*, pt.*, p.*
              FROM ""Menu"" as m
              LEFT OUTER JOIN ""MenuItem"" as mi on mi.menuId = m.menuId
              LEFT OUTER JOIN ""Product"" as p on p.productId = mi.productId
              LEFT OUTER JOIN ""ProductType"" as pt on pt.productTypeId = p.productTypeId
              WHERE m.venueId = @VenueId", new { VenueId = venueId }).ConfigureAwait(false);

                fullMenuDetails = GenerateFullMenuResults(data);

                if (data == null)
                {
                    return Result.Ok(Maybe<List<FullMenu>>.None);
                }
                return Result.Ok(Maybe<List<FullMenu>>.From(fullMenuDetails));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<FullMenu>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single menu
        /// </summary>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<FullMenu>>> FetchFullMenu(int venueId, int menuId)
        {
            try
            {
                var fullMenuDetails = new List<FullMenu>();
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var lookup = new Dictionary<int, FullMenu>();
                var data = await con.QueryMultipleAsync(
                  @"SELECT m.*, pt.*, p.*
              FROM ""Menu"" as m
              LEFT OUTER JOIN ""MenuItem"" as mi on mi.menuId = m.menuId
              LEFT OUTER JOIN ""Product"" as p on p.productId = mi.productId
              LEFT OUTER JOIN ""ProductType"" as pt on pt.productTypeId = p.productTypeId
              WHERE m.venueId = @VenueId
              AND m.menuId = @MenuId", new { VenueId = venueId, MenuId = menuId }).ConfigureAwait(false);

                fullMenuDetails = GenerateFullMenuResults(data);

                if (data == null)
                {
                    return Result.Ok(Maybe<FullMenu>.None);
                }

                return Result.Ok(Maybe<FullMenu>.From(fullMenuDetails.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<FullMenu>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single menu
        /// </summary>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<FullMenu>>> FetchFullMenu(int menuId)
        {
            try
            {
                var fullMenuDetails = new List<FullMenu>();
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var lookup = new Dictionary<int, FullMenu>();
                var data = await con.QueryMultipleAsync(
                  @"SELECT m.*, pt.*, p.*
              FROM ""Menu"" as m
              LEFT OUTER JOIN ""MenuItem"" as mi on mi.menuId = m.menuId
              LEFT OUTER JOIN ""Product"" as p on p.productId = mi.productId
              LEFT OUTER JOIN ""ProductType"" as pt on pt.productTypeId = p.productTypeId
              WHERE m.menuId = @MenuId", new { MenuId = menuId }).ConfigureAwait(false);

                fullMenuDetails = GenerateFullMenuResults(data);

                if (data == null)
                {
                    return Result.Ok(Maybe<FullMenu>.None);
                }

                return Result.Ok(Maybe<FullMenu>.From(fullMenuDetails.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<FullMenu>>(ex.ToString());
            }
        }

        /// <summary>
        /// Inserts a new Menu
        /// </summary>
        /// <param name="newMenu">The Menu to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result<int>> CreateFullMenu(NewMenu newMenu)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                int menuId = (await con.QueryAsync<int>(
                  @"INSERT INTO ""Menu""(
              venueId,
              menuDescription,
              menuName,
              menuImage
            ) VALUES (
              @VenueId,
              @MenuDescription,
              @MenuName,
              @MenuImage
            ) RETURNING menuId",
                  newMenu
                ).ConfigureAwait(false))
                  .FirstOrDefault();

                if (menuId < 1)
                {
                    return Result.Fail<int>("Failed to save menu");
                }

                await con.ExecuteAsync(
                  @"INSERT INTO ""MenuItem""(
              venueid, menuid, productid
            ) VALUES (
              @VenueId, @MenuId, @ProductId
            ) RETURNING menuId",
                  newMenu.Products.Select(p => new { newMenu.VenueId, MenuId = menuId, ProductId = p }).ToList()
                ).ConfigureAwait(false);

                return Result.Ok<int>(menuId);
            }
            catch (Exception ex)
            {
                return Result.Fail<int>(ex.ToString());
            }
        }

        /// <summary>
        /// Replaces a new Menu
        /// </summary>
        /// <param name="newMenu">The Menu to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result> ReplaceFullMenu(int venueId, UpdatedMenu newMenu)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync(
                  @"UPDATE ""Menu""
            SET menuDescription = @MenuDescription,
            menuName = @MenuName,
            menuImage = @MenuImage
            where menuid = @MenuId and venueid = @venueid",
                  new
                  {
                      newMenu.MenuDescription,
                      newMenu.MenuName,
                      newMenu.MenuImage,
                      newMenu.MenuId,
                      VenueId = venueId
                  }
                ).ConfigureAwait(false);

                await con.ExecuteAsync(@"DELETE FROM ""MenuItem"" WHERE menuId = @MenuId", new { newMenu.MenuId }).ConfigureAwait(false);

                await con.ExecuteAsync(
                  @"INSERT INTO ""MenuItem""(
              venueid, menuid, productid
            ) VALUES (
              @VenueId, @MenuId, @ProductId
            ) RETURNING menuId",
                  newMenu.Products.Select(p => new { venueId, newMenu.MenuId, ProductId = p }).ToList()
                ).ConfigureAwait(false);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes a single venue's Menu
        /// </summary>
        /// <param name="resourceId">The id of the Menu you wish to delete</param>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> DropVenueMenu(int venueId, int resourceId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync("DELETE FROM \"Menu\" WHERE menuId = @ResourceId AND venueId = @VenueId", new { VenueId = venueId, ResourceId = resourceId }).ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }

        private List<FullMenu> GenerateFullMenuResults(SqlMapper.GridReader data)
        {
            var results = new List<FullMenu>();

            var fullOrderItems = data.Read<FullMenuItemData>();

            var df = fullOrderItems.GroupBy(x => x.MenuId).
              ToDictionary(k => k.Key, k => k.ToList());

            foreach (var menuItem in df)
            {
                var orderDetails = menuItem.Value.FirstOrDefault();

                if (orderDetails != null)
                {
                    results.Add(new FullMenu()
                    {
                        MenuId = orderDetails.MenuId,
                        MenuDescription = orderDetails.MenuDescription,
                        MenuName = orderDetails.MenuName,
                        MenuImage = orderDetails.MenuImage,
                        VenueId = orderDetails.VenueId,
                        Products = new List<FullMenuItem>(menuItem.Value.Where(i => i.ProductName != null).Select(x => new FullMenuItem()
                        {
                            ProductId = x.ProductId,
                            ProductName = x.ProductName,
                            ProductType = x.ProductTypeName,
                            ProductDescription = x.ProductDescription,
                            AgeRestricted = x.AgeRestricted,
                            Image = x.Image,
                            Price = x.Price
                        }))
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// Fetches multiple Menus
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Menu>>>> FetchCountedVenueMenus(int venueId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"Menu\" WHERE venueId = @VenueId; SELECT * FROM \"Menu\" WHERE venueId = @VenueId LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count, VenueId = venueId }).ConfigureAwait(false);
                var totalCount = obj.Read<int>().Single();
                var data = obj.Read<Menu>().ToList();

                var paginatedData = new PaginatedResult<Menu>
                {
                    Data = data ?? new List<Menu>(),
                    Count = totalCount
                };

                return Result.Ok(Maybe<PaginatedResult<Menu>>.From(paginatedData));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<Menu>>>(ex.ToString());
            }
        }
    }
}
