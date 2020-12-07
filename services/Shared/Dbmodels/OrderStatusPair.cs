namespace Koasta.Shared.DbModels
{
    public class OrderStatusPair
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int OrderStatus { get; set; }
        public int OrderNumber { get; set; }
    }
}
