using System;
using System.Text;
using Koasta.Shared.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Koasta.Service.EventService.Consumers
{
    public class DelegatingOrderEventConsumer : IDisposable
    {
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;
        private readonly Shared.Types.Constants constants;
        private readonly ILogger logger;
        public delegate void OnNewEventHandler(object sender, string data);
        public event OnNewEventHandler OnNewEvent;
        private readonly int venueId;

        public DelegatingOrderEventConsumer(ISettings settings, Shared.Types.Constants constants, ILoggerFactory logger, int venueId)
        {
            this.venueId = venueId;
            this.logger = logger.CreateLogger(nameof(DelegatingOrderEventConsumer));
            this.constants = constants;
            this.Settings = settings;
        }

        protected string ExchangeName { get => $"{constants.OrderEventExchangeName}.{venueId}"; }
        protected string ExchangeType { get => "fanout"; }
        protected string QueueName { get => $"{constants.OrderEventExchangeName}.{venueId}.queue"; }
        protected string RoutingKey { get => $"{constants.OrderEventExchangeName}.{venueId}.publish"; }
        protected ISettings Settings { get; }

        protected void HandleMessage(string message)
        {
            bool result = TryHandleMessage(message);
            if (!result)
            {
                logger.LogError($"Failed to transmit message for order events queue: {message}");
            }
        }

        public void StartConsuming()
        {
            if (Settings.Connection.StubMessaging)
            {
                return;
            }

            var queueName = $"{QueueName}.{Guid.NewGuid()}";

            var factory = new ConnectionFactory()
            {
                HostName = Settings.Connection.RabbitMQHostname,
                UserName = Settings.Connection.RabbitMQUsername,
                Password = Settings.Connection.RabbitMQPassword
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare(ExchangeName, ExchangeType, true, false);

            channel.QueueDeclare(queueName, true, true, true, null);
            channel.QueueBind(queueName, ExchangeName, RoutingKey, null);

            consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                if (ea.Body.IsEmpty)
                {
                    return;
                }

                try
                {
                    var raw = Encoding.UTF8.GetString(ea.Body.ToArray());
                    HandleMessage(raw);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return;
                }
            };

            channel.BasicConsume(queueName, true, consumer);
        }

        protected IModel Channel => channel;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (consumer != null && channel != null)
                    {
                        foreach (var tag in consumer.ConsumerTags)
                        {
                            channel.BasicCancel(tag);
                        }

                        consumer = null;
                    }

                    if (channel != null)
                    {
                        channel.Abort();
                        channel.Dispose();
                        channel = null;
                    }

                    if (connection != null)
                    {
                        connection.Abort();
                        connection.Dispose();
                        connection = null;
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        private bool TryHandleMessage(string message)
        {
            try
            {
                OnNewEvent?.Invoke(this, message);
            }
            catch (Exception)
            {
                return false;
            }

            logger.LogDebug("Broadcasting order update message to web consumers");
            return true;
        }
    }
}
