using System.ComponentModel.DataAnnotations;

namespace Koasta.Service.CompanyService.Models
{
    public class DtoCreateCompanyRequest
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
        [Required]
        public bool BankAccountIsBusiness { get; set; }
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        public string SortCode { get; set; }
        [Required]
        public string NameOnAccount { get; set; }
    }
}
