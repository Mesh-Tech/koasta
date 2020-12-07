using System.Collections.Generic;

namespace Koasta.Service.OrderService.Models
{
    public class OrderCalculation
    {
        public decimal Total { get; set; }
        public decimal NetTotal { get; set; }
        public decimal PaymentProcessorFee { get; set; }
        public decimal KoastaFee { get; set; }
        public decimal GrossTotal { get; set; }
        public string Explanation { get; set; }
        public List<OrderCalculationLine> LineItems { get; set; }
    }
}
