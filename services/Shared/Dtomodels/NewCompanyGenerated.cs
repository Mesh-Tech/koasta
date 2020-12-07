using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class NewCompany
    {
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
    }
}
