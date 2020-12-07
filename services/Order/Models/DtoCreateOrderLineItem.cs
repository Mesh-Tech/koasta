namespace Koasta.Service.OrderService.Models
{
    public class DtoCreateOrderLineItem
    {
        public int VenueId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
