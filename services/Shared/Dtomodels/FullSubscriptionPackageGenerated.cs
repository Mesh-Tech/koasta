using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Koasta.Shared.Models
{
    [CompilerGeneratedAttribute()]
    public partial class FullSubscriptionPackage
    {
        [Column("packageId")]
        public int PackageId { get; set; }
        [Column("packageName")]
        public string PackageName { get; set; }
        [Column("externalPackageId")]
        public string ExternalPackageId { get; set; }
    }
}
