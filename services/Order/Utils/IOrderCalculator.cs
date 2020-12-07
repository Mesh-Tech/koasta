using CSharpFunctionalExtensions;
using Koasta.Service.OrderService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Koasta.Service.OrderService.Utils
{
    public interface IOrderCalculator
    {
        Task<Result<OrderCalculation>> Calculate(List<DtoCreateOrderLineItem> lineItems);
    }
}
