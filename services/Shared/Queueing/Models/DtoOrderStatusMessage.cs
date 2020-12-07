using Koasta.Shared.Types;

namespace Koasta.Shared.Queueing.Models
{
    public class DtoOrderStatusMessage
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public ServingType ServingType { get; set; }
    }
}
