using System.Collections.Generic;

namespace Koasta.Service.OrderService.Models
{
    public class DtoOrderEstimate
    {
        public List<OrderCalculationLine> ReceiptLines { get; set; }
        public decimal ReceiptTotal { get; set; }
        public string Explanation { get; set; }
    }
}
