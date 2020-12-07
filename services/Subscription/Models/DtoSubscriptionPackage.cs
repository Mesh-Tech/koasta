namespace Koasta.Service.SubscriptionService.Models
{
    public class DtoSubscriptionPackage
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public string Identifier { get; set; }
        public decimal MonthlyAmount { get; set; }
    }
}
