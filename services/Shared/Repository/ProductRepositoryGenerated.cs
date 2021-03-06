using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Configuration;
using Koasta.Shared.Models;
using Koasta.Shared.PatchModels;
using Koasta.Shared.Types;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

// WARNING: This file is auto-generated from {repository root}/scripts/csport/generate-repositories.js.
//          Do not edit this file directly, as changes will be replaced!

namespace Koasta.Shared.Database
{
    /*
    A repository for data access to Product resources.
    */
    [CompilerGeneratedAttribute()]
    public partial class ProductRepository : RepositoryBase<Product>
    {
        private readonly ISettings settings;
        private readonly ILogger logger;

        public ProductRepository(ISettings settings, ILoggerFactory logger) : base()
        {
            this.settings = settings;
            this.logger = logger.CreateLogger("ProductRepository");
        }

        /// <summary>
        /// Fetches multiple Products
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<List<Product>>>> FetchProducts(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<Product>("SELECT * FROM \"Product\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false)).ToList();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<List<Product>>.None);
                    }

                    return Result.Ok(Maybe<List<Product>>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<List<Product>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Products
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Product>>>> FetchCountedProducts(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"Product\"; SELECT * FROM \"Product\" LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false);
                    var totalCount = obj.Read<int>().Single();
                    var data = obj.Read<Product>().ToList();

                    var paginatedData = new PaginatedResult<Product> {
                      Data = data ?? new List<Product>(),
                      Count = totalCount
                    };

                    return Result.Ok(Maybe<PaginatedResult<Product>>.From(paginatedData));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<PaginatedResult<Product>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single Product
        /// </summary>
        /// <param name="resourceId">The id of the Product you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<Product>>> FetchProduct(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<Product>("SELECT * FROM \"Product\" WHERE productId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false)).FirstOrDefault();
                    if (data == null)
                    {
                        return Result.Ok(Maybe<Product>.None);
                    }

                    return Result.Ok(Maybe<Product>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<Product>>(ex.ToString());
            }
        }

        /// <summary>
        /// Inserts a new Product
        /// </summary>
        /// <param name="newProduct">The Product to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result<Maybe<int>>> CreateProduct(Product newProduct)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = (await con.QueryAsync<int>(
                        @"INSERT INTO ""Product""(
                            productTypeId,
                            productName,
                            productDescription,
                            price,
                            image,
                            ageRestricted,
                            parentProductId,
                            venueId
                        ) VALUES (
                            @ProductType,
                            @ProductName,
                            @ProductDescription,
                            @Price,
                            @Image,
                            @AgeRestricted,
                            @ParentProductId,
                            @VenueId
                        ) RETURNING productId",
                        newProduct
                    ).ConfigureAwait(false)).FirstOrDefault();
                    if (data < 1)
                    {
                        return Result.Ok(Maybe<int>.None);
                    }

                    return Result.Ok(Maybe<int>.From(data));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail<Maybe<int>>(ex.ToString());
            }
        }

        /// <summary>
        /// Deletes a single Product
        /// </summary>
        /// <param name="resourceId">The id of the Product you wish to delete</param>
        /// <returns>Returns a result indicating if the delete succeeded</returns>
        public async Task<Result> DropProduct(int resourceId)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync("DELETE FROM \"Product\" WHERE productId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false);
                    return Result.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Replaces a single Product with a new full set of values
        /// </summary>
        /// <param name="replacedProduct">The new data for the Product you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> ReplaceProduct(Product replacedProduct)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    await con.ExecuteAsync(
                        @"UPDATE ""Product""
                        SET
                        productTypeId = @ProductType,
                        productName = @ProductName,
                        productDescription = @ProductDescription,
                        price = @Price,
                        image = @Image,
                        ageRestricted = @AgeRestricted,
                        parentProductId = @ParentProductId,
                        venueId = @VenueId
                        WHERE productId = @ProductId",
                        replacedProduct
                    ).ConfigureAwait(false);

                    return Result.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Updates a single Product with one or more values
        /// </summary>
        /// <param name="updatedProduct">The new data for the Product you wish to update</param>
        /// <returns>Returns a result indicating if the operation succeeded</returns>
        public async Task<Result> UpdateProduct(ProductPatch updatedProduct)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var sqlPatchOperations = new StringBuilder();
                    var obj = updatedProduct;
                    var operationCount = 0;

                    if (obj.ProductType != null)
                    {
                        sqlPatchOperations.AppendLine(obj.ProductType.Operation == OperationKind.Remove 
                            ? "productTypeId = NULL,"
                            : "productTypeId = @ProductType,"
                        );
                        operationCount++;
                    }
                    if (obj.ProductName != null)
                    {
                        sqlPatchOperations.AppendLine(obj.ProductName.Operation == OperationKind.Remove 
                            ? "productName = NULL,"
                            : "productName = @ProductName,"
                        );
                        operationCount++;
                    }
                    if (obj.ProductDescription != null)
                    {
                        sqlPatchOperations.AppendLine(obj.ProductDescription.Operation == OperationKind.Remove 
                            ? "productDescription = NULL,"
                            : "productDescription = @ProductDescription,"
                        );
                        operationCount++;
                    }
                    if (obj.Price != null)
                    {
                        sqlPatchOperations.AppendLine(obj.Price.Operation == OperationKind.Remove 
                            ? "price = NULL,"
                            : "price = @Price,"
                        );
                        operationCount++;
                    }
                    if (obj.Image != null)
                    {
                        sqlPatchOperations.AppendLine(obj.Image.Operation == OperationKind.Remove 
                            ? "image = NULL,"
                            : "image = @Image,"
                        );
                        operationCount++;
                    }
                    if (obj.AgeRestricted != null)
                    {
                        sqlPatchOperations.AppendLine(obj.AgeRestricted.Operation == OperationKind.Remove 
                            ? "ageRestricted = NULL,"
                            : "ageRestricted = @AgeRestricted,"
                        );
                        operationCount++;
                    }
                    if (obj.ParentProductId != null)
                    {
                        sqlPatchOperations.AppendLine(obj.ParentProductId.Operation == OperationKind.Remove 
                            ? "parentProductId = NULL,"
                            : "parentProductId = @ParentProductId,"
                        );
                        operationCount++;
                    }
                    if (obj.VenueId != null)
                    {
                        sqlPatchOperations.AppendLine(obj.VenueId.Operation == OperationKind.Remove 
                            ? "venueId = NULL,"
                            : "venueId = @VenueId,"
                        );
                        operationCount++;
                    }

                    var patchOperations = sqlPatchOperations.ToString();

                    if (operationCount > 0)
                    {
                        // Remove final ", " from StringBuilder to ensure query is valid
                        patchOperations = patchOperations.TrimEnd(System.Environment.NewLine.ToCharArray());
                        patchOperations = patchOperations.TrimEnd(',');
                    }

                    await con.ExecuteAsync($"UPDATE \"Product\" SET {patchOperations} WHERE productId = @ResourceId", new {
                        ResourceId = obj.ResourceId,
                        ProductType = (int) (obj.ProductType == default ? default : obj.ProductType.Value),
                        ProductName = (string) (obj.ProductName == default ? default : obj.ProductName.Value),
                        ProductDescription = (string) (obj.ProductDescription == default ? default : obj.ProductDescription.Value),
                        Price = (decimal) (obj.Price == default ? default : obj.Price.Value),
                        Image = (string) (obj.Image == default ? default : obj.Image.Value),
                        AgeRestricted = (bool) (obj.AgeRestricted == default ? default : obj.AgeRestricted.Value),
                        ParentProductId = (int) (obj.ParentProductId == default ? default : obj.ParentProductId.Value),
                        VenueId = (int) (obj.VenueId == default ? default : obj.VenueId.Value)
                    }).ConfigureAwait(false);

                    return Result.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Query failed");
                return Result.Fail(ex.ToString());
            }
        }
    }
}
