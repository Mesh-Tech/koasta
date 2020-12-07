using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.DbModels;
using Koasta.Shared.Models;
using Koasta.Shared.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Koasta.Shared.Database
{
    public partial class OrderRepository : RepositoryBase<Order>
    {
        /// <summary>
        /// Inserts a new Order
        /// </summary>
        /// <param name="newOrder">The Order to be inserted</param>
        /// <returns>Returns a result containing the created resource id</returns>
        public async Task<Result<Maybe<int>>> CreateFullOrder(Order newOrder, List<OrderLine> newOrderLines)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<int>(
                  @"INSERT INTO ""CustomerOrder""(
              orderNumber,
              userId,
              venueId,
              orderStatus,
              orderTimeStamp,
              total,
              serviceCharge,
              nonce,
              ordernotes,
              servingtype, 
              tableName
            ) VALUES (
              @OrderNumber,
              @UserId,
              @VenueId,
              @OrderStatus,
              @OrderTimeStamp,
              @Total,
              @ServiceCharge,
              @Nonce,
              @OrderNotes,
              @ServingType,
              @Table
            ) RETURNING orderId",
                  newOrder
                ).ConfigureAwait(false)).FirstOrDefault();

                if (data < 1)
                {
                    return Result.Ok(Maybe<int>.None);
                }

                var result = await con.ExecuteAsync(
                  @"INSERT INTO ""OrderLine""(
             orderId, productId, quantity, amount
            ) VALUES (
              @OrderId, @ProductId, @Quantity, @Amount
            )",
                  newOrderLines.Select(p => new { OrderId = data, p.ProductId, p.Quantity, p.Amount }).ToList()
                ).ConfigureAwait(false);

                return Result.Ok(Maybe<int>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<int>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single menu
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<FullOrder>>> FetchFullOrder(int orderId)
        {
            try
            {
                var fullOrderDetails = new List<FullOrder>();
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = await con.QueryMultipleAsync(
                      @"SELECT c.orderId, c.orderNumber, c.orderNotes, u.userId, u.firstName, u.lastName, v.companyId, v.venueId, v.venueName, c.orderTimeStamp, c.orderStatus,
              p.productName, ol.orderLineId, ol.quantity, ol.amount, c.externalPaymentId, c.total, c.serviceCharge, c.orderNotes, c.servingType, c.tableName
              FROM ""CustomerOrder"" as c
              INNER JOIN ""User"" as u on u.userId = c.userId
              INNER JOIN ""Venue"" as v on v.venueId = c.venueId
              INNER JOIN ""OrderLine"" as ol on ol.orderId = c.orderId
              INNER JOIN ""Product"" as p on p.productId = ol.productId
              WHERE c.orderId = @OrderId", new { OrderId = orderId }).ConfigureAwait(false);

                    fullOrderDetails = GenerateFullOrderResults(data);

                    if (data == null)
                    {
                        return Result.Ok(Maybe<FullOrder>.None);
                    }
                }

                return Result.Ok(Maybe<FullOrder>.From(fullOrderDetails.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<FullOrder>>(ex.ToString());
            }
        }

        public async Task<Result<Maybe<FullOrder>>> FetchFullOrderByNonce(string nonce)
        {
            try
            {
                var fullOrderDetails = new List<FullOrder>();
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var data = await con.QueryMultipleAsync(
                      @"SELECT c.orderId, c.orderNumber, u.userId, u.firstName, u.lastName, v.companyId, v.venueName, v.venueId, c.orderTimeStamp, c.orderStatus,
              p.productName, ol.orderLineId, c.orderNotes, ol.quantity, ol.amount, c.externalPaymentId, c.total, c.serviceCharge, c.orderNotes, c.servingType, c.tableName
              FROM ""CustomerOrder"" as c
              INNER JOIN ""User"" as u on u.userId = c.userId
              INNER JOIN ""Venue"" as v on v.venueId = c.venueId
              INNER JOIN ""OrderLine"" as ol on ol.orderId = c.orderId
              INNER JOIN ""Product"" as p on p.productId = ol.productId
              WHERE c.nonce = @Nonce", new { Nonce = nonce }).ConfigureAwait(false);

                    fullOrderDetails = GenerateFullOrderResults(data);

                    if (data == null)
                    {
                        return Result.Ok(Maybe<FullOrder>.None);
                    }
                }

                return Result.Ok(Maybe<FullOrder>.From(fullOrderDetails.FirstOrDefault()));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<FullOrder>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a set of orders
        /// </summary>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<List<FullOrder>>> FetchFullOrders(int userId)
        {
            try
            {
                var fullOrderDetails = new List<FullOrder>();
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    var lookup = new Dictionary<int, FullOrder>();
                    var data = await con.QueryMultipleAsync(@"SELECT c.orderId, c.orderNumber, u.userId, u.firstName, u.lastName, v.companyId, v.venueId, v.venueName, c.orderTimeStamp, c.orderStatus, c.externalPaymentId,
              c.total, c.serviceCharge, c.orderNotes, c.servingType, c.tableName, ol.orderLineId, p.productName, ol.quantity, ol.amount
              FROM ""CustomerOrder"" as c
              INNER JOIN ""User"" as u on u.userId = c.userId
              INNER JOIN ""Venue"" as v on v.venueId = c.venueId
              INNER JOIN ""OrderLine"" as ol on ol.orderId = c.orderId
              INNER JOIN ""Product"" as p on p.productId = ol.productId
              WHERE u.userId = @UserId", new { UserId = userId }).ConfigureAwait(false);

                    fullOrderDetails = GenerateFullOrderResults(data);
                }

                return Result.Ok(fullOrderDetails);
            }
            catch (Exception ex)
            {
                return Result.Fail<List<FullOrder>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a set of orders
        /// </summary>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<List<FullOrder>>> FetchFullIncompleteOrders(int? companyId, int page, int count)
        {
            List<FullOrder> fullOrderDetails;

            var companyClause = companyId.HasValue
                      ? $" AND v.companyId = {companyId.Value}"
                      : "";

            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = await con.QueryMultipleAsync(
                  @"SELECT c.orderId, c.orderNumber, u.userId, u.firstName, u.lastName, v.companyId, v.venueId, v.venueName, c.orderTimeStamp, c.orderStatus, c.externalPaymentId,
              c.total, c.serviceCharge, c.orderNotes, c.servingType, c.tableName, ol.orderLineId, p.productName, ol.quantity, ol.amount
              FROM ""CustomerOrder"" as c
              INNER JOIN ""User"" as u on u.userId = c.userId
              INNER JOIN ""Venue"" as v on v.venueId = c.venueId
              INNER JOIN ""OrderLine"" as ol on ol.orderId = c.orderId
              INNER JOIN ""Product"" as p on p.productId = ol.productId
              WHERE c.orderStatus < 4 AND c.externalpaymentid IS NOT NULL" + companyClause + " LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page }).ConfigureAwait(false);

                fullOrderDetails = GenerateFullOrderResults(data);

                if (data == null)
                {
                    return Result.Ok(new List<FullOrder>());
                }

                return Result.Ok(fullOrderDetails);
            }
            catch (Exception ex)
            {
                return Result.Fail<List<FullOrder>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a set of orders
        /// </summary>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<List<FullOrder>>> FetchFullIncompleteVenueOrders(int venueId)
        {
            List<FullOrder> fullOrderDetails;

            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = await con.QueryMultipleAsync(
                  @"SELECT c.orderId, c.orderNumber, u.userId, u.firstName, u.lastName, v.companyId, v.venueId, v.venueName, c.orderTimeStamp, c.orderStatus, c.externalPaymentId,
              c.total, c.serviceCharge, c.orderNotes, c.servingType, c.tableName, ol.orderLineId, p.productName, ol.quantity, ol.amount
              FROM ""CustomerOrder"" as c
              INNER JOIN ""User"" as u on u.userId = c.userId
              INNER JOIN ""Venue"" as v on v.venueId = c.venueId
              INNER JOIN ""OrderLine"" as ol on ol.orderId = c.orderId
              INNER JOIN ""Product"" as p on p.productId = ol.productId
              WHERE c.orderStatus < 4 AND c.venueId = @VenueId AND c.externalpaymentid IS NOT NULL", new { VenueId = venueId }).ConfigureAwait(false);

                fullOrderDetails = GenerateFullOrderResults(data);

                if (data == null)
                {
                    return Result.Ok(new List<FullOrder>());
                }

                return Result.Ok(fullOrderDetails);
            }
            catch (Exception ex)
            {
                return Result.Fail<List<FullOrder>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a set of orders
        /// </summary>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<List<FullOrder>>> FetchFullCompleteOrders(int? companyId, int page, int count)
        {
            var companyClause = companyId.HasValue
                      ? $" AND v.companyId = {companyId.Value}"
                      : "";
            try
            {
                var fullOrderDetails = new List<FullOrder>();

                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = await con.QueryMultipleAsync(
                  @"SELECT c.orderId, c.orderNumber, u.userId, u.firstName, u.lastName, v.companyId, v.venueId, v.venueName, c.orderTimeStamp, c.orderStatus, c.externalPaymentId,
              c.total, c.serviceCharge, c.ordernotes, c.servingtype, c.tableName, ol.orderLineId, p.productName, ol.quantity, ol.amount
              FROM ""CustomerOrder"" as c
              INNER JOIN ""User"" as u on u.userId = c.userId
              INNER JOIN ""Venue"" as v on v.venueId = c.venueId
              INNER JOIN ""OrderLine"" as ol on ol.orderId = c.orderId
              INNER JOIN ""Product"" as p on p.productId = ol.productId
              WHERE c.orderStatus = 4" + companyClause + " LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page }).ConfigureAwait(false);

                fullOrderDetails = GenerateFullOrderResults(data);

                if (data == null)
                {
                    return Result.Ok(new List<FullOrder>());
                }

                return Result.Ok(fullOrderDetails);
            }
            catch (Exception ex)
            {
                return Result.Fail<List<FullOrder>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches a single order's status
        /// </summary>
        /// <param name="resourceId">The id of the Order you wish to fetch</param>
        /// <returns>Returns a result containing an optional item</returns>
        public async Task<Result<Maybe<OrderStatusPair>>> FetchOrderStatus(int resourceId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<OrderStatusPair>("SELECT o.orderId, o.orderStatus, o.orderNumber, o.userId, v.companyId FROM \"CustomerOrder\" o INNER JOIN \"Venue\" v ON v.venueId = o.venueId WHERE orderId = @ResourceId", new { ResourceId = resourceId }).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<OrderStatusPair>.None);
                }

                return Result.Ok(Maybe<OrderStatusPair>.From(data));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<OrderStatusPair>>(ex.ToString());
            }
        }

        private List<FullOrder> GenerateFullOrderResults(SqlMapper.GridReader data)
        {
            var results = new List<FullOrder>();

            var fullOrderItems = data.Read<FullOrderData>();

            var df = fullOrderItems.GroupBy(x => x.OrderId).
              ToDictionary(k => k.Key, k => k.ToList());

            foreach (var orderLine in df)
            {
                var orderDetails = orderLine.Value.FirstOrDefault();

                if (orderDetails != null)
                {
                    results.Add(new FullOrder()
                    {
                        CompanyId = orderDetails.CompanyId,
                        ExternalPaymentId = orderDetails.ExternalPaymentId,
                        FirstName = orderDetails.FirstName,
                        LastName = orderDetails.LastName,
                        OrderId = orderDetails.OrderId,
                        OrderNumber = orderDetails.OrderNumber,
                        OrderStatus = orderDetails.OrderStatus,
                        OrderTimeStamp = orderDetails.OrderTimeStamp,
                        UserId = orderDetails.UserId,
                        VenueName = orderDetails.VenueName,
                        VenueId = orderDetails.VenueId,
                        Total = orderDetails.Total,
                        ServiceCharge = orderDetails.ServiceCharge,
                        OrderNotes = orderDetails.OrderNotes,
                        ServingType = orderDetails.ServingType,
                        Table = orderDetails.Table,
                        LineItems = new List<FullOrderItem>
                      (orderLine.Value.Select
                      (x => new FullOrderItem() { Amount = x.Amount, Quantity = x.Quantity, Id = x.OrderLineId, ProductName = x.ProductName }))
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// Fetches multiple Orders
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Order>>>> FetchCountedCompleteOrders(int page, int count)
        {
            try
            {
                using (var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString))
                {
                    using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"CustomerOrder\" WHERE orderStatus = 4; SELECT * FROM \"CustomerOrder\" WHERE orderStatus = 4 LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count }).ConfigureAwait(false);
                    var totalCount = obj.Read<int>().Single();
                    var data = obj.Read<Order>().ToList();

                    var paginatedData = new PaginatedResult<Order>
                    {
                        Data = data ?? new List<Order>(),
                        Count = totalCount
                    };

                    return Result.Ok(Maybe<PaginatedResult<Order>>.From(paginatedData));
                }
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<Order>>>(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches multiple Orders
        /// </summary>
        /// <param name="page">The current page number</param>
        /// <param name="count">The page size</param>
        /// <returns>Returns a result containing an optional list of items</returns>
        public async Task<Result<Maybe<PaginatedResult<Order>>>> FetchCountedCompleteVenueOrders(int venueId, int page, int count)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                using var obj = await con.QueryMultipleAsync("SELECT COUNT(*) FROM \"CustomerOrder\" WHERE orderStatus = 4 AND venueId = @VenueId; SELECT * FROM \"CustomerOrder\" WHERE orderStatus = 4 AND venueId = @VenueId LIMIT @Limit OFFSET @Offset", new { Limit = count, Offset = page * count, VenueId = venueId }).ConfigureAwait(false);
                var totalCount = obj.Read<int>().Single();
                var data = obj.Read<Order>().ToList();

                var paginatedData = new PaginatedResult<Order>
                {
                    Data = data ?? new List<Order>(),
                    Count = totalCount
                };

                return Result.Ok(Maybe<PaginatedResult<Order>>.From(paginatedData));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<PaginatedResult<Order>>>(ex.ToString());
            }
        }
    }
}
