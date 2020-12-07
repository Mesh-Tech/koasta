using Newtonsoft.Json;

namespace Koasta.Shared.Configuration
{
    public class Data
    {
        [JsonProperty("database_url")]
        public string DatabaseConnectionString { get; set; }
        [JsonProperty("host_port")]
        public string HostPort { get; set; }
        [JsonProperty("aws_access_key_id")]
        public string AWSAccessKeyId { get; set; }
        [JsonProperty("aws_secret_access_key")]
        public string AWSSecretAccessKey { get; set; }
        [JsonProperty("square_app_id")]
        public string SquareAppId { get; set; }
        [JsonProperty("square_access_token")]
        public string SquareAccessToken { get; set; }
        [JsonProperty("square_sandbox")]
        public bool SquarePaymentsSandbox { get; set; }
        [JsonProperty("s3_bucket_name")]
        public string S3BucketName { get; set; }
        [JsonProperty("s3_private_bucket_name")]
        public string S3PrivateBucketName { get; set; }
        [JsonProperty("cf_private_root_endpoint")]
        public string CfPrivateRootEndpoint { get; set; }
        [JsonProperty("rabbitmq_hostname")]
        public string RabbitMQHostname { get; set; }
        [JsonProperty("rabbitmq_username")]
        public string RabbitMQUsername { get; set; }
        [JsonProperty("rabbitmq_password")]
        public string RabbitMQPassword { get; set; }
        [JsonProperty("stub_billing")]
        public bool StubBilling { get; set; }
        [JsonProperty("stub_messaging")]
        public bool StubMessaging { get; set; }
        [JsonProperty("redis_url")]
        public string RedisConnectionString { get; set; }
        [JsonProperty("memcached_url")]
        public string MemcachedUrl { get; set; }
        [JsonProperty("apple_push_key")]
        public string ApplePushKey { get; set; }
        [JsonProperty("apple_push_team_id")]
        public string ApplePushTeamId { get; set; }
        [JsonProperty("apple_push_key_id")]
        public string ApplePushKeyId { get; set; }
        [JsonProperty("apple_push_bundle_id")]
        public string ApplePushBundleId { get; set; }
        [JsonProperty("firebase_server_key")]
        public string FirebaseServerKey { get; set; }
        [JsonProperty("firebase_sender_id")]
        public string FirebaseSenderId { get; set; }
    }
}
