using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Koasta.Service.OrderService.Models;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Models;
using Koasta.Shared.PatchModels;
using Koasta.Shared.Queueing;
using Koasta.Shared.Types;
using Koasta.Shared.Billing;
using Koasta.Shared.Queueing.Models;
using Koasta.Service.OrderService.Utils;
using Koasta.Shared.Configuration;
using System.Collections.Generic;

namespace Koasta.Service.OrderService
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/order")]
    public class OrderController : Controller
    {
        private readonly OrderRepository orders;
        private readonly ProductRepository products;
        private readonly IMessagePublisher messagePublisher;
        private readonly Constants constants;
        private readonly IBillingManager billingManager;
        private readonly CompanyRepository companies;
        private readonly Random random;
        private readonly IOrderCalculator orderCalculator;
        private readonly IEnvironment environment;

        public OrderController(OrderRepository orders, ProductRepository products,
                               IMessagePublisher messagePublisher, Constants constants,
                               IBillingManager billingManager, CompanyRepository companies,
                               IOrderCalculator orderCalculator, IEnvironment environment)
        {
            this.orders = orders;
            this.products = products;
            this.messagePublisher = messagePublisher;
            this.constants = constants;
            this.billingManager = billingManager;
            this.companies = companies;
            this.random = new Random();
            this.orderCalculator = orderCalculator;
            this.environment = environment;
        }

        [HttpPut]
        [Route("{orderId}")]
        [ActionName("update_order_status")]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute(Name = "orderId")] int orderId, [FromBody] DtoUpdateOrderStatusRequest request)
        {
            var newStatus = OrderStatusFromString(request.StatusName);
            if (newStatus == OrderStatus.Unknown)
            {
                return BadRequest();
            }

            var orderResult = await orders.FetchOrder(orderId)
              .Ensure(o => o.HasValue, "Order exists")
              .OnSuccess(o => o.Value)
              .ConfigureAwait(false);

            if (orderResult.IsFailure)
            {
                return NotFound();
            }

            var order = orderResult.Value;

            if (newStatus == OrderStatus.Complete && order.Total > 0)
            {
                await billingManager.FinaliseOrderPayment(order).ConfigureAwait(false);
            }

            return await orders.UpdateOrder(new OrderPatch
            {
                ResourceId = order.OrderId,
                OrderStatus = new PatchOperation<int>
                {
                    Value = (int)OrderStatusFromString(request.StatusName),
                    Operation = OperationKind.Update
                }
            })
            .OnSuccess(() => messagePublisher.Publish(
              $"{constants.OrderEventExchangeName}.{order.VenueId}",
              PublishStrategy.Fanout,
              new Message<DtoOrderStatusMessage>
              {
                  Type = newStatus == OrderStatus.Rejected ? constants.MessageOrderCancelled : constants.MessageOrderUpdated,
                  Data = new DtoOrderStatusMessage
                  {
                      OrderId = orderId,
                      OrderStatus = request.StatusName,
                      ServingType = (ServingType) order.ServingType
                  }
              }
            ))
            .OnSuccess(() =>
            {
                var servingType = (ServingType) order.ServingType;
                var requestedTableService = servingType != ServingType.BarService && !string.IsNullOrWhiteSpace(order.Table);
                string notification = newStatus switch
                {
                    OrderStatus.Ordered => "We've received your order. Your order will be prepared shortly.",
                    OrderStatus.InProgress => "Your order's being prepared. It'll be ready to collect shortly.",
                    OrderStatus.Ready => requestedTableService ? "Your order will be with you shortly. Thanks for using Koasta!" : "Your order's ready to collect! Show your receipt at the bar to pick it up.",
                    _ => null,
                };
                if (notification == null)
                {
                    return;
                }

                messagePublisher.DirectPublish(
            constants.NotificationQueueName,
            constants.NotificationExchangeRoutingKey,
            constants.NotificationExchangeName,
            constants.NotificationExchangeQueueName,
            new Message<DtoNotificationMessage>
                {
                    Type = constants.MessageGeneric,
                    Data = new DtoNotificationMessage
                    {
                        NotificationType = constants.NotificationTypeOrderUpdate,
                        UserId = order.UserId,
                        Message = notification,
                    }
                }
          );
            })
            .OnBoth(x => x.IsFailure ? StatusCode(500) : StatusCode(200))
            .ConfigureAwait(false);
        }

        [HttpPost]
        [Route("estimate")]
        [ActionName("estimate_order")]
        [ProducesResponseType(typeof(DtoOrderEstimate), 200)]
        public async Task<IActionResult> EstimateOrder([FromBody] DtoEstimateOrderRequest request)
        {
            if (this.GetAuthContext().User.HasNoValue && this.GetAuthContext().Employee.HasNoValue)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid || request.OrderLines.Count < 1)
            {
                return BadRequest();
            }

            var estimate = await orderCalculator.Calculate(request.OrderLines).ConfigureAwait(false);
            if (estimate.IsFailure)
            {
                return BadRequest();
            }

            return Ok(new DtoOrderEstimate
            {
                ReceiptLines = estimate.Value.LineItems,
                ReceiptTotal = estimate.Value.Total,
                Explanation = environment.IsProduction ? null : estimate.Value.Explanation
            });
        }

        [HttpPost]
        [ActionName("create_order")]
        [ProducesResponseType(typeof(DtoNewOrderStatus), 200)]
        public async Task<IActionResult> CreateOrder([FromBody] DtoCreateOrderRequest request)
        {
            if (this.GetAuthContext().User.HasNoValue)
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid || request.OrderLines.Count < 1)
            {
                return BadRequest();
            }

            var estimate = await orderCalculator.Calculate(request.OrderLines).ConfigureAwait(false);
            if (estimate.IsFailure)
            {
                return BadRequest();
            }

            var storeOrderResult = await StoreOrder(request, estimate.Value)
                .ConfigureAwait(false);

            if (storeOrderResult.IsFailure)
            {
                return StatusCode(500);
            }

            var reservePaymentResult = await ReserveOrderPayment(storeOrderResult.Value, estimate.Value, request.PaymentProcessorReference, request.PaymentVerificationReference)
                .ConfigureAwait(false);

            if (reservePaymentResult.IsFailure)
            {
                await orders.UpdateOrder(new OrderPatch
                {
                    ResourceId = storeOrderResult.Value.OrderId,
                    OrderStatus = new PatchOperation<int> { Value = (int) OrderStatus.PaymentFailed, Operation = OperationKind.Update }
                }).ConfigureAwait(false);
                return StatusCode(500);
            }

            messagePublisher.Publish(
                $"{constants.OrderEventExchangeName}.{storeOrderResult.Value.VenueId}",
                PublishStrategy.Fanout,
                new Message<Order>
                {
                    Type = constants.MessageOrderCreated,
                    Data = storeOrderResult.Value
                }
            );

            return StatusCode(200, (DtoNewOrderStatus)storeOrderResult.Value);
        }

        [HttpGet]
        [ActionName("fetch_full_order")]
        [Route("{orderId}")]
        [ProducesResponseType(typeof(FullOrder), 200)]
        public async Task<IActionResult> GetFullOrder([FromRoute(Name = "orderId")] int orderId)
        {
            var auth = this.GetAuthContext();

            if (auth.UserType == UserType.Employee && auth.EmployeeRole.HasNoValue)
            {
                return NotFound();
            }

            return await orders.FetchFullOrder(orderId)
              .Ensure(o => o.HasValue, "Order exists")
              .OnSuccess(o =>
              {
                  if (auth.UserType == UserType.Employee
              && auth.Employee.Value.CompanyId != o.Value.CompanyId
              && !auth.EmployeeRole.Value.CanAdministerSystem)
                  {
                      return Result.Fail<FullOrder>("Companies cannot view other companies' orders if they're not sysadmins");
                  }
                  else if (auth.UserType == UserType.User
                  && auth.User.Value.UserId != o.Value.UserId)
                  {
                      return Result.Fail<FullOrder>("Users cannot view other users' orders");
                  }

                  return Result.Ok<FullOrder>(o.Value);
              })
              .OnBoth(o => o.IsFailure ? StatusCode(404, "") : StatusCode(200, o.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_full_incomplete_orders")]
        [Route("incomplete")]
        [ProducesResponseType(typeof(List<FullOrder>), 200)]
        public async Task<IActionResult> GetFullIncompleteOrders([FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            var auth = this.GetAuthContext();

            if (auth.UserType == UserType.Employee && auth.EmployeeRole.HasNoValue)
            {
                return NotFound();
            }

            var companyId = auth.EmployeeRole.Value.CanAdministerSystem
              ? (int?)null
              : auth.Employee.Value.CompanyId;

            return await orders.FetchFullIncompleteOrders(companyId, page, count)
              .OnBoth(o => o.IsFailure ? StatusCode(404, "") : StatusCode(200, o.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_full_incomplete_orders")]
        [Route("incomplete/{venueId}")]
        [ProducesResponseType(typeof(List<FullOrder>), 200)]
        public async Task<IActionResult> GetFullIncompleteVenueOrders(int venueId)
        {
            var auth = this.GetAuthContext();

            if (auth.UserType == UserType.Employee && auth.EmployeeRole.HasNoValue)
            {
                return NotFound();
            }

            return await orders.FetchFullIncompleteVenueOrders(venueId)
              .OnBoth(o => o.IsFailure ? StatusCode(404, "") : StatusCode(200, o.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_full_complete_orders")]
        [Route("complete")]
        [ProducesResponseType(typeof(List<FullOrder>), 200)]
        public async Task<IActionResult> GetFullCompleteOrders([FromQuery(Name = "page")] int page = 0, [FromQuery(Name = "count")] int count = 20)
        {
            var auth = this.GetAuthContext();

            if (auth.UserType == UserType.Employee && auth.EmployeeRole.HasNoValue)
            {
                return NotFound();
            }

            var companyId = auth.EmployeeRole.Value.CanAdministerSystem
              ? (int?)null
              : auth.Employee.Value.CompanyId;

            return await orders.FetchFullCompleteOrders(companyId, page, count)
              .OnBoth(o => o.IsFailure ? StatusCode(404, "") : StatusCode(200, o.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_orders")]
        [ProducesResponseType(typeof(List<FullOrder>), 200)]
        public async Task<IActionResult> GetOrders()
        {
            return await orders.FetchFullOrders(this.GetAuthContext().User.Value.UserId)
              .OnBoth(o => o.IsFailure ? StatusCode(404, "") : StatusCode(200, o.Value))
              .ConfigureAwait(false);
        }

        [HttpGet]
        [ActionName("fetch_order_status")]
        [Route("{orderId}/status")]
        [ProducesResponseType(typeof(DtoOrderStatus), 200)]
        public async Task<IActionResult> GetOrderStatus([FromRoute(Name = "orderId")] int orderId)
        {
            var auth = this.GetAuthContext();

            if (auth.UserType == UserType.Employee && auth.EmployeeRole.HasNoValue)
            {
                return NotFound();
            }

            return await orders.FetchOrderStatus(orderId)
              .Ensure(o => o.HasValue, "Order exists")
              .OnSuccess(o =>
              {
                  if (auth.UserType == UserType.Employee
              && auth.Employee.Value.CompanyId != o.Value.CompanyId
              && !auth.EmployeeRole.Value.CanAdministerSystem)
                  {
                      return Result.Fail<DtoOrderStatus>("Companies cannot view other companies' orders if they're not sysadmins");
                  }
                  else if (auth.UserType == UserType.User
                  && auth.User.Value.UserId != o.Value.UserId)
                  {
                      return Result.Fail<DtoOrderStatus>("Users cannot view other users' orders");
                  }

                  return Result.Ok<DtoOrderStatus>((DtoOrderStatus)o.Value);
              })
              .OnBoth(o => o.IsFailure ? StatusCode(404, "") : StatusCode(200, o.Value))
              .ConfigureAwait(false);
        }

        private OrderStatus OrderStatusFromString(string data)
        {
            return data switch
            {
                "Unknown" => OrderStatus.Unknown,
                "Ordered" => OrderStatus.Ordered,
                "InProgress" => OrderStatus.InProgress,
                "Ready" => OrderStatus.Ready,
                "Complete" => OrderStatus.Complete,
                "Rejected" => OrderStatus.Rejected,
                "PaymentPending" => OrderStatus.PaymentPending,
                "PaymentFailed" => OrderStatus.PaymentFailed,
                _ => OrderStatus.Unknown,
            };
        }

        private async Task<Result<Order>> StoreOrder(DtoCreateOrderRequest request, OrderCalculation summary)
        {
            var existingOrder = await orders.FetchFullOrderByNonce(request.Nonce)
                .Ensure(o => o.HasValue, "Order found")
                .OnSuccess(o => o.Value)
                .ConfigureAwait(false);

            if (existingOrder.IsSuccess)
            {
                var ret = new Order
                {
                    OrderId = existingOrder.Value.OrderId,
                    OrderNumber = existingOrder.Value.OrderNumber,
                    UserId = existingOrder.Value.UserId ?? 0,
                    VenueId = existingOrder.Value.VenueId,
                    OrderStatus = existingOrder.Value.OrderStatus,
                    OrderTimeStamp = existingOrder.Value.OrderTimeStamp,
                    Total = existingOrder.Value.Total,
                    ServiceCharge = existingOrder.Value.ServiceCharge,
                    OrderNotes = existingOrder.Value.OrderNotes,
                    LineItems = existingOrder.Value.LineItems.Select(i => new OrderLine
                    {
                        OrderId = existingOrder.Value.OrderId,
                        OrderLineId = i.Id,
                        Amount = i.Amount,
                        Quantity = i.Quantity
                    }).ToList(),
                    ServingType = existingOrder.Value.ServingType,
                    Table = existingOrder.Value.Table
                };

                return Result.Ok(ret);
            }

            var items = summary.LineItems.Where(li => li.ProductId > 0).Select(li => new OrderLine
            {
                ProductId = li.ProductId,
                Quantity = li.Quantity,
                Amount = li.Amount
            }).ToList();

            var newOrder = new Order
            {
                OrderNumber = random.Next(1, 9999),
                UserId = this.GetAuthContext().User.Value.UserId,
                VenueId = request.OrderLines[0].VenueId,
                OrderStatus = (int)OrderStatus.PaymentPending,
                OrderTimeStamp = DateTime.UtcNow,
                Total = summary.Total,
                ServiceCharge = summary.KoastaFee + summary.PaymentProcessorFee,
                Nonce = request.Nonce,
                OrderNotes = request.OrderNotes,
                ServingType = (int) request.ServingType,
                Table = request.Table
            };

            return await orders.CreateFullOrder(newOrder, items)
            .Ensure(o => o.HasValue, "Order created successfully")
            .OnSuccess(o =>
            {
                newOrder.OrderId = o.Value;
                newOrder.LineItems = items;
                return newOrder;
            })
            .ConfigureAwait(false);
        }

        private async Task<Result<Order>> ReserveOrderPayment(Order order, OrderCalculation summary, string paymentProcessorReference, string paymentVerificationReference)
        {
            var status = (OrderStatus)order.OrderStatus;
            if ((status != OrderStatus.Ordered && status != OrderStatus.PaymentFailed && status != OrderStatus.PaymentPending) || !string.IsNullOrWhiteSpace(order.ExternalPaymentId))
            {
                return Result.Ok(order);
            }

            var companyResult = await companies.FetchCompanyByVenue(order.VenueId)
              .Ensure(c => c.HasValue, "Company exists")
              .ConfigureAwait(false);

            if (companyResult.IsFailure)
            {
                return Result.Fail<Order>("Unable to fetch company for order");
            }

            var company = companyResult.Value.Value;
            string externalPaymentId = null;
            var billingOrder = new BillingOrder
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderedAt = order.OrderTimeStamp,
                VenueId = company.VenueId,
                VenueName = company.VenueName,
                LineItems = summary.LineItems.Select(li => new BillingOrderLineItem
                {
                    ProductId = li.ProductId,
                    Quantity = li.Quantity,
                    BasePrice = li.Amount
                }).ToList(),
                Total = summary.Total,
                GrossTotal = summary.GrossTotal,
                PaymentProcessorFee = summary.PaymentProcessorFee,
                KoastaFee = summary.KoastaFee
            };

            if (summary.Total > 0)
            {
                try
                {
                    externalPaymentId = await billingManager.ReserveOrderPayment(
                      this.GetAuthContext().User.Value,
                      billingOrder,
                      paymentProcessorReference,
                      company.ExternalAccountId,
                      paymentVerificationReference
                    ).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return Result.Fail<Order>(ex.ToString());
                }
            }
            else
            {
                externalPaymentId = $"FREE-{Guid.NewGuid()}";
            }

            order.ExternalPaymentId = externalPaymentId;
            order.OrderStatus = (int) OrderStatus.Ordered;

            return await orders.UpdateOrder(new OrderPatch
            {
                ResourceId = order.OrderId,
                ExternalPaymentId = new PatchOperation<string>
                {
                    Value = externalPaymentId,
                    Operation = OperationKind.Update
                },
                OrderStatus = new PatchOperation<int>
                {
                    Value = (int) OrderStatus.Ordered,
                    Operation = OperationKind.Update
                }
            })
            .OnSuccess(() => order)
            .ConfigureAwait(false);
        }
    }
}
