namespace Koasta.Shared.Billing
{
    public class BillingOrderLineItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal BasePrice { get; set; }
    }
}
