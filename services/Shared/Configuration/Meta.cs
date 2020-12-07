using Newtonsoft.Json;
using System.Collections.Generic;

namespace Koasta.Shared.Configuration
{
    public class Meta
    {
        [JsonProperty("path_base")]
        public string PathBase { get; set; }
        [JsonProperty("requires_message_queue")]
        public bool RequiresMessageQueue { get; set; }
        [JsonProperty("debug")]
        public bool Debug { get; set; }
        [JsonProperty("api_auth_requirements")]
        public Dictionary<string, ApiAuthRequirement> AuthRequirements { get; set; }
        [JsonProperty("transaction_fee_percentage")]
        public decimal TransactionFeePercentage { get; set; }
        [JsonProperty("transaction_fee_minimum")]
        public decimal TransactionFeeMinimum { get; set; }
        [JsonProperty("payment_processor_fee_percentage")]
        public decimal PaymentProcessorFeePercentage { get; set; }
        [JsonProperty("enable_access_token_encryption")]
        public bool EnableAccessTokenEcryption { get; set; }
    }
}
