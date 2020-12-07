using Newtonsoft.Json;

namespace Koasta.Shared.Configuration
{
    public class Auth
    {
        [JsonProperty("hash_cost")]
        public int HashCost { get; set; }
        [JsonProperty("auth_token_validity_minutes")]
        public int AuthTokenValidityMinutes { get; set; }
        [JsonProperty("refresh_token_validity_minutes")]
        public int RefreshTokenValidityMinutes { get; set; }
        [JsonProperty("twilio_account_id")]
        public string TwilioAccountId { get; set; }
        [JsonProperty("twilio_auth_token")]
        public string TwilioAuthToken { get; set; }
        [JsonProperty("twilio_phone_number")]
        public string TwilioPhoneNumber { get; set; }
        [JsonProperty("facebook_auth_verify_url")]
        public string FacebookAuthVerifyAddress { get; set; }
        [JsonProperty("mailjet_api_key")]
        public string MailjetApiKey { get; set; }
        [JsonProperty("mailjet_secret_key")]
        public string MailjetSecretKey { get; set; }
    }
}
