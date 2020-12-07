using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class NewSubscriptionPlan
    {
        [Column("companyId")]
        public int CompanyId { get; set; }
        [Column("externalPlanId")]
        public string ExternalPlanId { get; set; }
    }
}
