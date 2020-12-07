using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Koasta.Shared.Queueing;
using Koasta.Shared.Queueing.Models;
using Koasta.Shared.Types;
using System.Linq;
using CorePush.Apple;
using Polly;
using CorePush.Google;
using System.Globalization;

namespace Koasta.Service.EventService.Consumers
{
    public class NotificationConsumer : AbstractQueueConsumer<Message<DtoNotificationMessage>>
    {
        private readonly ISettings settings;
        private readonly DeviceRepository devices;
        private readonly Shared.Types.Constants constants;

        internal class ApsPayload
        {
            [JsonProperty("alert")]
            public string Alert { get; set; }
        }

        internal class AppleNotification
        {
            [JsonProperty("aps")]
            public ApsPayload Aps { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("params")]
            public Dictionary<string, string> Params { get; set; }
        }
        
        class FirebaseAndroidNotification
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("channel_id")]
            public string ChannelId { get; set; }
        }

        class FirebaseNotificationInfo
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }
        }

        class FirebasePayload
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("params")]
            public Dictionary<string, string> Params { get; set; }
        }

        class FirebaseAndroidPayload
        {
            [JsonProperty("data")]
            public FirebasePayload Data { get; set; }

            [JsonProperty("notification")]
            public FirebaseAndroidNotification Notification { get; set; }

            [JsonProperty("priority")]
            public int Priority { get; set; }
        }

        class FirebaseNotification
        {
            [JsonProperty("android")]
            public FirebaseAndroidPayload Android { get; set; }

            [JsonProperty("notification")]
            public FirebaseNotificationInfo Notification { get; set; }
        }

        public NotificationConsumer(ISettings settings,
                                    ILoggerFactory logger,
                                    DeviceRepository devices,
                                    Constants constants) : base(logger)
        {
            this.settings = settings;
            this.devices = devices;
            this.constants = constants;
        }

        protected override string QueueName => constants.NotificationQueueName;

        protected override ISettings Settings => settings;

        protected override string DeadletterQueueName => constants.NotificationExchangeQueueName;

        protected override string DeadletterExchangeName => constants.NotificationExchangeName;

        protected override string DeadletterRoutingKey => constants.NotificationExchangeRoutingKey;

        protected override async Task HandleMessage(Message<DtoNotificationMessage> message)
        {
            var result = await devices.FetchDevicesForUser(message.Data.UserId)
              .Ensure(devices => devices.HasValue, "Devices were found")
              .OnSuccess(devices => SendNotificationsForDevices(devices.Value, message.Data.Message, message.Data.NotificationType, message.Data.Payload))
              .ConfigureAwait(false);

            if (result.IsSuccess)
            {
                Logger.LogDebug($"Notifications sent for user: {message.Data.UserId}");
            }
            else
            {
                throw new InvalidOperationException("Sending notifications failed");
            }
        }

        private async Task SendNotificationsForDevices(List<Device> devices, string message, string type, Dictionary<string, string> payload)
        {
            if (devices.Count == 0)
            {
                return;
            }

            var iosPayload = new AppleNotification
            {
                Aps = new ApsPayload
                {
                    Alert = message,
                },
                Type = type,
                Params = payload,
            };

            var androidPayload = new FirebaseNotification
            {
                Android = new FirebaseAndroidPayload
                {
                    Notification = new FirebaseAndroidNotification
                    {
                        Body = message,
                        ChannelId = "GENERAL"
                    },
                    Priority = 10,
                    Data = new FirebasePayload
                    {
                        Type = type,
                        Params = payload
                    }
                },
                Notification = new FirebaseNotificationInfo
                {
                    Body = message
                }
            };

            var iOSDevices = devices.Where(d => d.Platform == 1).ToList();

            using var apn = new ApnSender(
                settings.Connection.ApplePushKey,
                settings.Connection.ApplePushKeyId,
                settings.Connection.ApplePushTeamId,
                settings.Connection.ApplePushBundleId,
                ApnServerType.Production
            );

            await Task.WhenAll(iOSDevices.Select(device => Policy
                .HandleResult<ApnsResponse>(r => !r.IsSuccess)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(8)
                }, (exception, timeSpan) =>
                {
                    var time = timeSpan.ToString("h'h 'm'm 's's'", CultureInfo.CurrentCulture);
                    Logger.LogWarning($"Failed to send iOS notification to device id {device.Token} in {time}: {exception?.Result?.Error?.Reason}");
                })
                .ExecuteAsync(() => apn.SendAsync(iosPayload, device.Token))
            ).ToArray())
            .ConfigureAwait(false);

            var androidDevices = devices.Where(d => d.Platform == 2).ToList();

            using var fcm = new FcmSender(settings.Connection.FirebaseServerKey, settings.Connection.FirebaseSenderId);
            await Task.WhenAll(androidDevices.Select(device => Policy
                .HandleResult<FcmResponse>(r => r.Failure > 0)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(8)
                }, (_, timeSpan) =>
                {
                    var time = timeSpan.ToString("h'h 'm'm 's's'", CultureInfo.CurrentCulture);
                    Logger.LogWarning($"Failed to send Android notification to device id {device.Token} in {time}");
                })
                .ExecuteAsync(() => fcm.SendAsync(device.Token, androidPayload))
            ).ToArray())
            .ConfigureAwait(false);
        }
    }
}
