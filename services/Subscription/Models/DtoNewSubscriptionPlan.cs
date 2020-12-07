using System.Collections.Generic;

namespace Koasta.Service.SubscriptionService.Models
{
    public class DtoNewSubscriptionPlan
    {
        public int CompanyId { get; set; }
        public List<int> Packages { get; set; }
    }
}
