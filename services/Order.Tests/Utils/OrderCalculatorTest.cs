using CSharpFunctionalExtensions;
using Koasta.Service.OrderService.Models;
using Koasta.Service.OrderService.Utils;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Koasta.Service.Order.Tests.Utils
{
    public class OrderEstimateTestData
    {
        public List<DtoCreateOrderLineItem> LineItems { get; set; }
        public List<Product> Products { get; set; }
        public decimal TransactionFeePercentage { get; set; } = 0.08M;
        public decimal TransactionFeeMinimum { get; set; } = 0.4M;
        public decimal PaymentProcessorFeePercentage { get; set; } = 0.025M;
        public decimal ExpectedTotal { get; set; }
        public decimal ExpectedNetTotal { get; set; }
        public decimal ExpectedAppFee { get; set; }

        public Result<Maybe<Dictionary<int, Product>>> GenerateProductDictionary()
        {
            var ret = new Dictionary<int, Product>();
            Products.ForEach(p => ret[p.ProductId] = p);
            return Result.Ok(Maybe<Dictionary<int, Product>>.From(ret));
        }
    }

    public class FakeSettings : ISettings
    {
        public Data Connection { get; set; }

        public Meta Meta { get; set; }

        public Auth Auth { get; set; }

        public void Dispose()
        {
        }
    }

    public class OrderCalculatorTest
    {
        [Theory]
        [MemberData(nameof(GetData), parameters: 1)]
        public async Task OrderEstimateIsAccurate(OrderEstimateTestData data)
        {
            var mockLoggerFactory = new LoggerFactory();
            var mockProducts = new Mock<ProductRepository>(null, mockLoggerFactory);
            var stubSettings = new FakeSettings
            {
                Meta = new Meta
                {
                    TransactionFeeMinimum = data.TransactionFeeMinimum,
                    TransactionFeePercentage = data.TransactionFeePercentage,
                    PaymentProcessorFeePercentage = data.PaymentProcessorFeePercentage
                }
            };

            mockProducts.Setup(_ => _.FetchProducts(It.IsAny<List<int>>())).Returns(Task.FromResult(data.GenerateProductDictionary()));
            var calculator = new OrderCalculator(mockProducts.Object, stubSettings);

            var result = await calculator.Calculate(data.LineItems).ConfigureAwait(false);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(data.ExpectedTotal, result.Value.Total);
            Assert.Equal(data.ExpectedAppFee, result.Value.KoastaFee);
            Assert.Equal(data.ExpectedNetTotal, result.Value.NetTotal);
        }

        public static IEnumerable<object[]> GetData(int _)
        {
            return new List<object[]>
            {
                new object[] { new OrderEstimateTestData {
                    LineItems = new List<DtoCreateOrderLineItem> { new DtoCreateOrderLineItem { VenueId = 0, ProductId = 0, Quantity = 1 } },
                    Products = new List<Product> { new Product { ProductId = 0, Price = 10 } },
                    ExpectedTotal = 10.8M,
                    ExpectedNetTotal = 10M,
                    ExpectedAppFee = 0.53M
                } },
                new object[] { new OrderEstimateTestData {
                    LineItems = new List<DtoCreateOrderLineItem> { new DtoCreateOrderLineItem { VenueId = 0, ProductId = 0, Quantity = 1 } },
                    Products = new List<Product> { new Product { ProductId = 0, Price = 1 } },
                    ExpectedTotal = 1.40M,
                    ExpectedNetTotal = 1M,
                    ExpectedAppFee = 0.36M
                } },
                new object[] { new OrderEstimateTestData {
                    LineItems = new List<DtoCreateOrderLineItem> { new DtoCreateOrderLineItem { VenueId = 0, ProductId = 0, Quantity = 1 } },
                    Products = new List<Product> { new Product { ProductId = 0, Price = 5.5M } },
                    ExpectedTotal = 5.94M,
                    ExpectedNetTotal = 5.5M,
                    ExpectedAppFee = 0.29M
                } },
                new object[] { new OrderEstimateTestData {
                    LineItems = new List<DtoCreateOrderLineItem> { new DtoCreateOrderLineItem { VenueId = 0, ProductId = 0, Quantity = 1 } },
                    Products = new List<Product> { new Product { ProductId = 0, Price = 5.4M } },
                    ExpectedTotal = 5.83M,
                    ExpectedNetTotal = 5.40M,
                    ExpectedAppFee = 0.28M
                } },
                new object[] { new OrderEstimateTestData {
                    LineItems = new List<DtoCreateOrderLineItem> { new DtoCreateOrderLineItem { VenueId = 0, ProductId = 0, Quantity = 1 } },
                    Products = new List<Product> { new Product { ProductId = 0, Price = 21.25M } },
                    ExpectedTotal = 22.95M,
                    ExpectedNetTotal = 21.25M,
                    ExpectedAppFee = 1.13M
                } },
                new object[] { new OrderEstimateTestData {
                    LineItems = new List<DtoCreateOrderLineItem> { new DtoCreateOrderLineItem { VenueId = 0, ProductId = 0, Quantity = 1 } },
                    Products = new List<Product> { new Product { ProductId = 0, Price = 9.95M } },
                    ExpectedTotal = 10.74M,
                    ExpectedNetTotal = 9.95M,
                    ExpectedAppFee = 0.52M
                } },
                new object[] { new OrderEstimateTestData {
                    LineItems = new List<DtoCreateOrderLineItem> { new DtoCreateOrderLineItem { VenueId = 0, ProductId = 0, Quantity = 1 } },
                    Products = new List<Product> { new Product { ProductId = 0, Price = 7.77M } },
                    ExpectedTotal = 8.39M,
                    ExpectedNetTotal = 7.77M,
                    ExpectedAppFee = 0.41M
                } },
                new object[] { new OrderEstimateTestData {
                    LineItems = new List<DtoCreateOrderLineItem> { new DtoCreateOrderLineItem { VenueId = 0, ProductId = 0, Quantity = 1 } },
                    Products = new List<Product> { new Product { ProductId = 0, Price = 0.10M } },
                    ExpectedTotal = 0.5M,
                    ExpectedNetTotal = 0.10M,
                    ExpectedAppFee = 0.39M
                } },
                new object[] { new OrderEstimateTestData {
                    LineItems = new List<DtoCreateOrderLineItem> { new DtoCreateOrderLineItem { VenueId = 0, ProductId = 0, Quantity = 1 } },
                    Products = new List<Product> { new Product { ProductId = 0, Price = 3 } },
                    ExpectedTotal = 3.40M,
                    ExpectedNetTotal = 3M,
                    ExpectedAppFee = 0.31M
                } },
                new object[] { new OrderEstimateTestData {
                    LineItems = new List<DtoCreateOrderLineItem> { new DtoCreateOrderLineItem { VenueId = 0, ProductId = 0, Quantity = 1 } },
                    Products = new List<Product> { new Product { ProductId = 0, Price = 6.55M } },
                    ExpectedTotal = 7.07M,
                    ExpectedNetTotal = 6.55M,
                    ExpectedAppFee = 0.34M
                } }
            };
        }
    }
}
