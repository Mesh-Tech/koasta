namespace Koasta.Shared.Models
{
    public enum OrderStatus
    {
        Unknown = 0,
        Ordered = 1,
        InProgress = 2,
        Ready = 3,
        Complete = 4,
        Rejected = 5,
        PaymentPending = 6,
        PaymentFailed = 7
    }

    public class TotalOrderStatus
    {
        public int NumberOfOrders { get; set; }
    }

    public class CompletedTotalOrderStatus : TotalOrderStatus
    {
        public OrderStatus OrderStatus { get; set; }

        public string OrderStatusStringValue { get => OrderStatus.ToString(); }
    }
}
