using Koasta.Shared.Models;

namespace Koasta.Service.EmployeeService.Models
{
    public class DtoEmployee
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Username { get; set; }
        public int CompanyId { get; set; }
        public int VenueId { get; set; }
        public int RoleId { get; set; }

        public static explicit operator DtoEmployee(Employee employee)
        {
            return new DtoEmployee
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName = employee.EmployeeName,
                Username = employee.Username,
                CompanyId = employee.CompanyId,
                VenueId = employee.VenueId,
                RoleId = employee.RoleId,
            };
        }
    }
}
