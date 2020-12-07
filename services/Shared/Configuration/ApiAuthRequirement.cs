using Newtonsoft.Json;
using Koasta.Shared.Types;

namespace Koasta.Shared.Configuration
{
    public class ApiAuthRequirement
    {
        [JsonProperty("user_type")]
        public UserType UserType { get; set; }
        [JsonProperty("administer_system")]
        public bool AdministerSystem { get; set; }
        [JsonProperty("administer_company")]
        public bool AdministerCompany { get; set; }
        [JsonProperty("administer_venue")]
        public bool AdministerVenue { get; set; }
        [JsonProperty("work_with_company")]
        public bool WorkWithCompany { get; set; }
        [JsonProperty("work_with_venue")]
        public bool WorkWithVenue { get; set; }
    }
}
