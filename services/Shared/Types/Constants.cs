namespace Koasta.Shared.Types
{
    public sealed class Constants
    {
        public string OrderEventExchangeName { get; } = "orders";
        public string NotificationQueueName { get; } = "notifications.queue";
        public string VenueSyncQueueName { get; } = "venuesync.queue";
        public string AccessTokenRenewalQueueName { get; } = "tokenrenewal.queue";
        public string NotificationExchangeName { get; } = "notifications.deadletter.exchange";
        public string NotificationExchangeQueueName { get; } = "notifications.deadletter.queue";
        public string NotificationExchangeRoutingKey { get; } = "notifications.deadletter.key";
        public string VenueSyncExchangeName { get; } = "venuesync.deadletter.exchange";
        public string AccessTokenRenewalExchangeName { get; } = "tokenrenewal.deadletter.exchange";
        public string VenueSyncExchangeQueueName { get; } = "venuesync.deadletter.queue";
        public string AccessTokenRenewalExchangeQueueName { get; } = "tokenrenewal.deadletter.queue";
        public string VenueSyncExchangeRoutingKey { get; } = "venuesync.deadletter.key";
        public string AccessTokenRenewalExchangeRoutingKey { get; } = "tokenrenewal.deadletter.key";
        public string MessageGeneric { get; } = "generic";
        public string MessageOrderCreated { get; } = "orderCreated";
        public string MessageOrderUpdated { get; } = "orderUpdated";
        public string VenueSynch { get; } = "syncVenue";
        public string AccessTokenRenewal { get; } = "TokenRenewal";
        public string MessageOrderCancelled { get; } = "orderCancelled";
        public string MessageSendNotification { get; } = "sendNotification";
        public string NotificationTypeOrderUpdate { get; } = "orderUpdate";
        public string AesIV256Key { get; } = "/Koasta/DataProtection/AesIV256Key";
        public string Aes256Key { get; } = "/Koasta/DataProtection/AesKey256Key";
    }
}
