using System.ComponentModel.DataAnnotations;

namespace Koasta.Service.CompanyService.Models
{
    public class DtoUpdateCompanyRequest
    {
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string CompanyAddress { get; set; }
        [Required]
        public string CompanyPostcode { get; set; }
        public string CompanyContact { get; set; }
        [Required]
        public string CompanyPhone { get; set; }
        [Required]
        public string CompanyEmail { get; set; }

        public bool BankAccountIsBusiness { get; set; }
        public string AccountNumber { get; set; }
        public string SortCode { get; set; }
        public string NameOnAccount { get; set; }
    }
}
