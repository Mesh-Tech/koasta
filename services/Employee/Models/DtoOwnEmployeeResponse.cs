namespace Koasta.Service.EmployeeService.Models
{
    public class DtoOwnEmployeeResponse
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string Username { get; set; }
        public int CompanyId { get; set; }
        public int VenueId { get; set; }
        public int RoleId { get; set; }
        public bool CanWorkWithVenue { get; set; }
        public bool CanAdministerVenue { get; set; }
        public bool CanWorkWithCompany { get; set; }
        public bool CanAdministerCompany { get; set; }
        public bool CanAdministerSystem { get; set; }
    }
}
