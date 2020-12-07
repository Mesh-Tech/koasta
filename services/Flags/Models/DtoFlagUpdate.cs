using System.ComponentModel.DataAnnotations;

namespace Koasta.Service.Flags.Models
{
    public class DtoFlagUpdate
    {
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public bool Value { get; set; }
    }
}
