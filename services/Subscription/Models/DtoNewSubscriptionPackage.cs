namespace Koasta.Service.SubscriptionService.Models
{
    public class DtoNewSubscriptionPackage
    {
        public string PackageName { get; set; }
        public string Identifier { get; set; }
        public decimal MonthlyAmount { get; set; }
    }
}
