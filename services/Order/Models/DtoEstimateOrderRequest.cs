using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Koasta.Service.OrderService.Models
{
    public class DtoEstimateOrderRequest
    {
        [Required]
        public List<DtoCreateOrderLineItem> OrderLines { get; set; }
    }
}
