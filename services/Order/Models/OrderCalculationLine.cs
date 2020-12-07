namespace Koasta.Service.OrderService.Models
{
    public class OrderCalculationLine
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public decimal Total { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
