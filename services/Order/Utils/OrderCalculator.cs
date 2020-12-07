using CSharpFunctionalExtensions;
using Koasta.Service.OrderService.Models;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koasta.Service.OrderService.Utils
{
    public class OrderCalculator : IOrderCalculator
    {
        private readonly ProductRepository products;
        private readonly ISettings settings;

        public OrderCalculator(ProductRepository products, ISettings settings)
        {
            this.products = products;
            this.settings = settings;
        }

        public async Task<Result<OrderCalculation>> Calculate(List<DtoCreateOrderLineItem> lineItems)
        {
            var pricesResult = await products.FetchProducts(lineItems.Select(li => li.ProductId).ToList())
              .Ensure(p => p.HasValue, "Prices were found")
              .OnSuccess(p =>p.Value)
              .ConfigureAwait(false);

            if (!pricesResult.IsSuccess)
            {
                return Result.Fail<OrderCalculation>("Failed to find products for line items");
            }

            var explanation = new StringBuilder();

            var total = lineItems.Select(i => pricesResult.Value[i.ProductId].Price * i.Quantity).Sum() * 100;
            if (total == 0) {
                explanation.Append("Total charge is £0.00 as the user only selected free items");

                var lines = lineItems.Select(i => new OrderCalculationLine
                {
                    Title = pricesResult.Value[i.ProductId].ProductName,
                    Amount = pricesResult.Value[i.ProductId].Price,
                    Total = i.Quantity * pricesResult.Value[i.ProductId].Price,
                    Quantity = i.Quantity,
                    ProductId = i.ProductId
                }).ToList();

                return Result.Ok(new OrderCalculation
                {
                    Total = 0,
                    NetTotal = 0,
                    PaymentProcessorFee = 0,
                    KoastaFee = 0,
                    Explanation = explanation.ToString(),
                    LineItems = lines
                });
            }

            explanation.Append("Total charge for all product selections is ").Append(total).Append("p.    ");

            var transactionFeeMinimumInPence = settings.Meta.TransactionFeeMinimum * 100;

            var appFee = Math.Round(total * settings.Meta.TransactionFeePercentage, 0, MidpointRounding.ToZero);

            if (appFee < transactionFeeMinimumInPence)
            {
                explanation
                    .AppendLine("As the app fee is ")
                    .Append(appFee)
                    .Append("p, we adjust this to meet our minimum fee of ")
                    .Append(transactionFeeMinimumInPence)
                    .Append("p.    ");
                appFee = transactionFeeMinimumInPence;
            }
            else
            {
                explanation
                    .Append("App fee is total * ")
                    .Append(settings.Meta.TransactionFeePercentage)
                    .Append(" which works out to ")
                    .Append(appFee)
                    .Append("p.    ");
            }

            var totalChargedAmount = total + appFee;
            explanation
                .Append("Our total charged amount is ")
                .Append(total)
                .Append("p + ")
                .Append(appFee)
                .Append("p which works out to a total of ")
                .Append(totalChargedAmount)
                .Append("p.    ");

            var squareTxnFee = Math.Round(totalChargedAmount * settings.Meta.PaymentProcessorFeePercentage, 0, MidpointRounding.AwayFromZero);

            var netTotal = totalChargedAmount - squareTxnFee;
            explanation
                .Append("The total amount that would go to the business before taking our fee, minus Square's txn fee is ")
                .Append(totalChargedAmount)
                .Append("p * (1 - ")
                .Append(settings.Meta.PaymentProcessorFeePercentage)
                .Append(") which works out to ")
                .Append(netTotal)
                .Append("p.    ");

            var squareFeeAmount = totalChargedAmount - netTotal;
            explanation
                .Append("The total charged amount '")
                .Append(totalChargedAmount)
                .Append("', minus the net total '")
                .Append(netTotal)
                .Append("', gives us ")
                .Append(squareFeeAmount)
                .Append(" for the Square fee amount.    ");

            var appFeeMoney = appFee - squareFeeAmount;
            explanation
                .Append("As we pay Square transaction fees, our app fee is now ")
                .Append(appFeeMoney)
                .Append(".");

            var calcLines = lineItems.Select(i => new OrderCalculationLine
            {
                Title = pricesResult.Value[i.ProductId].ProductName,
                Amount = pricesResult.Value[i.ProductId].Price,
                Total = i.Quantity * pricesResult.Value[i.ProductId].Price,
                Quantity = i.Quantity,
                ProductId = i.ProductId
            }).ToList();

            calcLines.Add(new OrderCalculationLine
            {
                Title = "Service charge",
                Amount = 0,
                Quantity = 0,
                Total = appFee / 100,
                ProductId = 0
            });

            return Result.Ok(new OrderCalculation
            {
                Total = totalChargedAmount / 100,
                NetTotal = total / 100,
                PaymentProcessorFee = squareFeeAmount / 100,
                KoastaFee = appFeeMoney / 100,
                Explanation = explanation.ToString(),
                LineItems = calcLines
            });
        }
    }
}
