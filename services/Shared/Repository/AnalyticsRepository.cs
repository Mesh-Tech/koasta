using CSharpFunctionalExtensions;
using Dapper;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Koasta.Shared.Models;

namespace Koasta.Shared.Repository
{
    public class AnalyticsRepository : RepositoryBase<AverageOrderValue>
    {
        private readonly ISettings settings;

        public AnalyticsRepository(ISettings settings) : base()
        {
            this.settings = settings;
        }

        public async Task<Result<Maybe<List<AverageOrderValue>>>> GetAverageOrderAmount(int companyId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var today = DateTime.Today;

                var startTime = today;
                var endTime = today.AddDays(1).AddTicks(-1);

                var data = await con.QueryAsync<AverageOrderValue>(@"select v.venueId, count(v.venueid) as orders, sum(co.total) as total, sum(co.total) / count(v.venueid) as average from ""Venue"" v 
                    inner join ""CustomerOrder"" co 
                    on co.venueid = v.venueid 
                    where co.total > 0 
                    and co.ordertimestamp >= @StartTime 
                    and co.ordertimestamp < @EndTime 
                    and v.companyid = @CompanyId
                    group by v.venueid", new { CompanyId = companyId, StartTime = startTime, EndTime = endTime }).ConfigureAwait(true);
                if (data == null)
                {
                    return Result.Ok(Maybe<List<AverageOrderValue>>.None);
                }

                return Result.Ok(Maybe<List<AverageOrderValue>>.From(new List<AverageOrderValue>(data)));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<AverageOrderValue>>>(ex.ToString());
            }
        }

        public async Task<Result<Maybe<List<CompanyOrderStatus>>>> GetCompanyOrderStatuses(int companyId)
        {
            try
            {
                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var today = DateTime.Today;

                var startTime = today;
                var endTime = today.AddDays(1).AddTicks(-1);

                var data = await con.QueryAsync<CompanyOrderStatus>(@"select v.venueId, count(co.orderid) filter (where co.orderstatus < 4) as incomplete, count(co.orderid) filter (where co.orderstatus = 4) as complete, count(co.orderid) filter (where co.orderstatus > 4) as failed, count(co.orderid) as total from ""Venue"" v
                    inner join ""CustomerOrder"" co
                    on co.venueid = v.venueid
                    where co.ordertimestamp >= @StartTime
                    and co.ordertimestamp < @EndTime
                    and v.companyid = @CompanyId
                    group by v.venueid", new { CompanyId = companyId, StartTime = startTime, EndTime = endTime }).ConfigureAwait(true);
                if (data == null)
                {
                    return Result.Ok(Maybe<List<CompanyOrderStatus>>.None);
                }

                return Result.Ok(Maybe<List<CompanyOrderStatus>>.From(new List<CompanyOrderStatus>(data)));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<List<CompanyOrderStatus>>>(ex.ToString());
            }
        }

        public async Task<Result<Maybe<CompletedTotalOrderStatus>>> GetCompletedOrderStatusForVenue(int venueId, int orderStatusValue)
        {
            try
            {
                var today = DateTime.Today;

                var startTime = today;
                var endTime = today.AddDays(1).AddTicks(-1);

                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<int?>("select * from \"CustomerOrder\" as o where venueid = @VenueId and orderstatus = @orderStatus and o.ordertimestamp >= @StartTime AND o.ordertimestamp < @EndTime ", new { VenueId = venueId, OrderStatus = orderStatusValue, StartTime = startTime, EndTime = endTime }).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<CompletedTotalOrderStatus>.None);
                }

                var orderStatus = new CompletedTotalOrderStatus() { NumberOfOrders = data.Value, OrderStatus = GetOrderStatus(orderStatusValue) };

                return Result.Ok(Maybe<CompletedTotalOrderStatus>.From(orderStatus));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<CompletedTotalOrderStatus>>(ex.ToString());
            }
        }

        public async Task<Result<Maybe<TotalOrderStatus>>> GetActiveOrderStatusForVenue(int venueId)
        {
            try
            {
                var today = DateTime.Today;

                var startTime = today;
                var endTime = today.AddDays(1).AddTicks(-1);

                using var con = new Npgsql.NpgsqlConnection(settings.Connection.DatabaseConnectionString);
                var data = (await con.QueryAsync<int?>("select * from \"CustomerOrder\" o where venueid = @VenueId and orderstatus < 4 and o.ordertimestamp >= @StartTime AND o.ordertimestamp < @EndTime ", new { VenueId = venueId, StartTime = startTime, EndTime = endTime }).ConfigureAwait(false)).FirstOrDefault();
                if (data == null)
                {
                    return Result.Ok(Maybe<TotalOrderStatus>.None);
                }

                var orderStatus = new TotalOrderStatus() { NumberOfOrders = data.Value };

                return Result.Ok(Maybe<TotalOrderStatus>.From(orderStatus));
            }
            catch (Exception ex)
            {
                return Result.Fail<Maybe<TotalOrderStatus>>(ex.ToString());
            }
        }

        private OrderStatus GetOrderStatus(int orderStatus)
        {
            return orderStatus switch
            {
                1 => OrderStatus.Ordered,
                2 => OrderStatus.InProgress,
                3 => OrderStatus.Ready,
                4 => OrderStatus.Complete,
                5 => OrderStatus.Rejected,
                6 => OrderStatus.PaymentPending,
                7 => OrderStatus.PaymentFailed,
                _ => OrderStatus.Unknown,
            };
        }
    }
}
