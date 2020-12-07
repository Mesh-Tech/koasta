using System.Collections.Generic;

namespace Koasta.Service.SubscriptionService.Models
{
    public class DtoSubscriptionPlan
    {
        public int PlanId { get; set; }
        public int CompanyId { get; set; }
        public string ExternalPlanId { get; set; }
        public List<DtoSubscriptionPackage> Packages { get; set; }
    }
}
