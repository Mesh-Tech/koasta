using System.Collections.Generic;

namespace Koasta.Service.UserService.Models
{
    public class DtoUserProfile
    {
        public string RegistrationId { get; set; }
        public bool WantAdvertising { get; set; }
        public List<int> VotedVenueIds { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
