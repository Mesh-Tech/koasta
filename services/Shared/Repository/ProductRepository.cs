using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Models;
using Koasta.Shared.PatchModels;
using System.Text;
using Koasta.Shared.DbModels;
using Koasta.Shared.Types;

namespace Koasta.Shared.Database
{
    public partial class ProductRepository : RepositoryBase<Product>
    {
        /// <summary>
        /// Fetches a dictionary of productIds and their prices
        /// </summary>
        /// <param name="productIds">The list of products to query</param>
        /// <returns>Returns a result containing an optional dictionary of prices</returns>
        public virtual async Task<Result<Maybe<Dictionary<int, decimal>>>> FetchProductPrices(List<int> productIds)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<ProductPricePair>("SELECT productId, price FROM \"Product\" WHERE productId = ANY (@ProductIds)", new { ProductIds = productIds }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<Dictionary<int, decimal>>.None);
                }

                var priceDirectory = data.ToDictionary(pair => pair.ProductId, pair => pair.Price);

                return Result.Ok(Maybe<Dictionary<int, decimal>>.From(priceDirectory));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<Dictionary<int, decimal>>>(ex.ToString());
            }
        }

        /// <summary>
         /// Fetches a dictionary of productIds and their information
         /// </summary>
         /// <param name="productIds">The list of products to query</param>
         /// <returns>Returns a result containing an optional dictionary of prices</returns>
        public virtual async Task<Result<Maybe<Dictionary<int, Product>>>> FetchProducts(List<int> productIds)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Product>("SELECT * FROM \"Product\" WHERE productId = ANY (@ProductIds)", new { ProductIds = productIds }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<Dictionary<int, Product>>.None);
                }

                var priceDirectory = new Dictionary<int, Product>();
                data.ForEach(p => priceDirectory[p.ProductId] = p);

                return Result.Ok(Maybe<Dictionary<int, Product>>.From(priceDirectory));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<Dictionary<int, Product>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Products
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Product>>>> FetchVenueProducts(int venueId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Product>("SELECT * FROM \"Product\" WHERE venueId = @VenueId LIMIT @Limit OFFSET @Offset", new { VenueId = venueId, Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                if (data == null)
                {
                    return Result.Ok(Maybe<List<Product>>.None);
                }

                return Result.Ok(Maybe<List<Product>>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<Product>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single Product
        /// </summary>
        /// <param name="resourceId">The id of the Product you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<Product>>> FetchVenueProduct(int venueId, int resourceId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<Product>("SELECT * FROM \"Product\" WHERE venueId = @VenueId AND productId = @ResourceId", new { VenueId = venueId, ResourceId = resourceId }).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<Product>.None);
                }

                return Result.Ok(Maybe<Product>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<Product>>(ex.ToString());
            }
        }

        /// <summary>
        /// Inserts a new Product
        /// </summary>
        /// <param name="newProduct">The Product to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result<Maybe<int>>> CreateVenueProduct(NewProduct newProduct)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<int>(
                  @"INSERT INTO ""Product""(
              venueId,
              productTypeId,
              productName,
              productDescription,
              price,
              image,
              ageRestricted,
              parentProductId
            ) VALUES (
              @VenueId,
              @ProductTypeId,
              @ProductName,
              @ProductDescription,
              @Price,
              @Image,
              @AgeRestricted,
              @ParentProductId
            ) RETURNING productId",
                  newProduct
                ).ConfigureAwait(false)).FirstOrDefault();
                if (data < 1)
                {
                    return Result.Ok(Maybe<int>.None);
                }

                return Result.Ok(Maybe<int>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<int>>(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes a single Product
        /// </summary>
        /// <param name="resourceId">The id of the Product you wish to delete</param>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> DropVenueProduct(int venueId, int resourceId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync("DELETE FROM \"Product\" WHERE venueId = @VenueId AND productId = @ResourceId", new { VenueId = venueId, ResourceId = resourceId }).ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Replaces a single Product with a new full set of values
        /// </summary>
        /// <param name="replacedProduct">The new data for the Product you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> ReplaceVenueProduct(UpdatedProduct replacedProduct)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                await con.ExecuteAsync(
                  @"UPDATE ""Product""
            SET
            productTypeId = @ProductTypeId,
            productName = @ProductName,
            productDescription = @ProductDescription,
            price = @Price,
            image = @Image,
            ageRestricted = @AgeRestricted,
            parentProductId = @ParentProductId
            WHERE venueId = @VenueId AND productId = @ProductId",
                  replacedProduct
                ).ConfigureAwait(false);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Updates a single Product with one or more values
        /// </summary>
        /// <param name="updatedProduct">The new data for the Product you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> UpdateVenueProduct(int venueId, ProductPatch updatedProduct)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var sqlPatchOperations = new StringBuilder();
                var obj = updatedProduct;

                if (obj.ProductType != null)
                {
                    sqlPatchOperations.AppendLine(obj.ProductType.Operation == OperationKind.Remove
                      ? "productTypeId = NULL,"
                      : "productTypeId = @ProductType,"
                    );
                }
                if (obj.ProductName != null)
                {
                    sqlPatchOperations.AppendLine(obj.ProductName.Operation == OperationKind.Remove
                      ? "productName = NULL,"
                      : "productName = @ProductName,"
                    );
                }
                if (obj.ProductDescription != null)
                {
                    sqlPatchOperations.AppendLine(obj.ProductDescription.Operation == OperationKind.Remove
                      ? "productDescription = NULL,"
                      : "productDescription = @ProductDescription,"
                    );
                }
                if (obj.Price != null)
                {
                    sqlPatchOperations.AppendLine(obj.Price.Operation == OperationKind.Remove
                      ? "price = NULL,"
                      : "price = @Price,"
                    );
                }
                if (obj.Image != null)
                {
                    sqlPatchOperations.AppendLine(obj.Image.Operation == OperationKind.Remove
                      ? "image = NULL,"
                      : "image = @Image,"
                    );
                }
                if (obj.AgeRestricted != null)
                {
                    sqlPatchOperations.AppendLine(obj.AgeRestricted.Operation == OperationKind.Remove
                      ? "ageRestricted = NULL,"
                      : "ageRestricted = @AgeRestricted,"
                    );
                }
                if (obj.ParentProductId != null)
                {
                    sqlPatchOperations.AppendLine(obj.ParentProductId.Operation == OperationKind.Remove
                      ? "parentProductId = NULL"
                      : "parentProductId = @ParentProductId"
                    );
                }

                // Remove final ", " from StringBuilder to ensure query is valid
                sqlPatchOperations.Remove(sqlPatchOperations.Length - 2, 2);

                await con.ExecuteAsync($"UPDATE \"Product\" SET {sqlPatchOperations} WHERE venueId = @VenueId AND productId = @ResourceId", new
                {
                    VenueId = venueId,
                    obj.ResourceId,
                    ProductType = (int)(obj.ProductType == default ? default : obj.ProductType.Value),
                    ProductName = (string)(obj.ProductName == default ? default : obj.ProductName.Value),
                    ProductDescription = (string)(obj.ProductDescription == default ? default : obj.ProductDescription.Value),
                    Price = (decimal)(obj.Price == default ? default : obj.Price.Value),
                    Image = (string)(obj.Image == default ? default : obj.Image.Value),
                    AgeRestricted = (bool)(obj.AgeRestricted != default && obj.AgeRestricted.Value),
                    ParentProductId = (int)(obj.ParentProductId == default ? default : obj.ParentProductId.Value)
                }).ConfigureAwait(false);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Products
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Product>>>> FetchCountedVenueProducts(int venueId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"Product\" WHERE venueId = @VenueId; SELECT * FROM \"Product\" WHERE venueId = @VenueId LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count, VenueId = venueId }).ConfigureAwait(false);
                var totalCount = obj.Read<int>().Single();
                var data = obj.Read<Product>().ToList();

                var paginatedData = new PaginatedResult<Product>
                {
                    Data = data ?? new List<Product>(),
                    Count = totalCount
                };

                return Result.Ok(Maybe<PaginatedResult<Product>>.From(paginatedData));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<Product>>>(ex.ToString());
            }
        }
    }
}
