using System.ComponentModel.DataAnnotations;

namespace Koasta.Service.UserService.Models
{
    public class DtoNewDeviceModel
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public int Platform { get; set; }
    }
}
