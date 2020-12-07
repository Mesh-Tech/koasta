using Koasta.Shared.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Koasta.Service.OrderService.Models
{
    public class DtoCreateOrderRequest
    {
        public string PaymentProcessorReference { get; set; }
        public string PaymentVerificationReference { get; set; }
        [Required]
        public List<DtoCreateOrderLineItem> OrderLines { get; set; }
        [Required]
        public string Nonce { get; set; }
        public string OrderNotes { get; set; }
        [Required]
        public ServingType ServingType { get; set; }
        public string Table { get; set; }
    }
}
