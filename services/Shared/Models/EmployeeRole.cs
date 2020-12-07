namespace Koasta.Shared.Models
{
    public partial class EmployeeRole
    {
        public bool IsMorePrivilegedThanRole(EmployeeRole other)
        {
            if (CanAdministerSystem && !other.CanAdministerSystem)
            {
                return true;
            }

            if (CanAdministerCompany && !other.CanAdministerCompany)
            {
                return true;
            }

            if (CanWorkWithCompany && !other.CanWorkWithCompany)
            {
                return true;
            }

            if (CanAdministerVenue && !other.CanAdministerVenue)
            {
                return true;
            }

            if (CanWorkWithVenue && !other.CanWorkWithVenue)
            {
                return true;
            }

            return false;
        }
    }
}
