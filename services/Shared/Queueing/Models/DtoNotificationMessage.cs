using System.Collections.Generic;

namespace Koasta.Shared.Queueing.Models
{
    public class DtoNotificationMessage
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public Dictionary<string, string> Payload { get; set; }
    }
}
