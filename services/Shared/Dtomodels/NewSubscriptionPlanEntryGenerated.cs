using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class NewSubscriptionPlanEntry
    {
        [Column("packageId")]
        public int PackageId { get; set; }
    }
}
