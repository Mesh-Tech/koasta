using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class Company
    {
        [Column("companyId")]
        public int CompanyId { get; set; }
        [Column("companyName")]
        public string CompanyName { get; set; }
        [Column("companyAddress")]
        public string CompanyAddress { get; set; }
        [Column("companyPostcode")]
        public string CompanyPostcode { get; set; }
        [Column("companyContact")]
        public string CompanyContact { get; set; }
        [Column("companyPhone")]
        public string CompanyPhone { get; set; }
        [Column("companyEmail")]
        public string CompanyEmail { get; set; }
        [Column("externalAccountId")]
        public string ExternalAccountId { get; set; }
        [Column("externalCustomerId")]
        public string ExternalCustomerId { get; set; }
        [Column("externalAccessToken")]
        public string ExternalAccessToken { get; set; }
        [Column("externalRefreshToken")]
        public string ExternalRefreshToken { get; set; }
        [Column("externalTokenExpiry")]
        public DateTime ExternalTokenExpiry { get; set; }
        [Column("referenceCode")]
        public string ReferenceCode { get; set; }
    }
}
