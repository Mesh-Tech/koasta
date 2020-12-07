using System.ComponentModel.DataAnnotations;

namespace Koasta.Service.EmployeeService.Models
{
    public class DtoCreateEmployeeRequest
    {
        [Required]
        public int VenueId { get; set; }
        [Required]
        public int CompanyId { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string EmployeeName { get; set; }
        [Required]
        public int RoleId { get; set; }
    }
}
