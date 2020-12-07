using Newtonsoft.Json;

namespace Koasta.Service.EventService.Middleware
{
    internal class WebsocketStatusMessage
    {
        [JsonProperty("authToken")]
        public string AuthToken { get; set; }
    }
}
