using System;
using System.Collections.Generic;

namespace Koasta.Shared.Billing
{
    public class BillingOrder
    {
        public int OrderId { get; set; }
        public int OrderNumber { get; set; }
        public DateTime OrderedAt { get; set; }
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public List<BillingOrderLineItem> LineItems { get; set; }
        public decimal Total { get; set; }
        public decimal PaymentProcessorFee { get; set; }
        public decimal KoastaFee { get; set; }
        public decimal GrossTotal { get; set; }
    }
}
