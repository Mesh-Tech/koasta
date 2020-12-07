using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Koasta.Shared.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Koasta.Service.EventService.Consumers
{
    public abstract class AbstractQueueConsumer<T> : IConsumer, IDisposable where T : class
    {
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;

        protected abstract string QueueName { get; }
        protected abstract string DeadletterQueueName { get; }
        protected abstract string DeadletterExchangeName { get; }
        protected abstract string DeadletterRoutingKey { get; }
        protected abstract ISettings Settings { get; }
        protected string ConsumerUuid { get; }

        protected IModel Channel => channel;
        protected ILogger Logger { get; }

        protected abstract Task HandleMessage(T message);

        protected AbstractQueueConsumer(ILoggerFactory logger)
        {
            this.Logger = logger.CreateLogger("QueueConsumer");
        }

        public void StartConsuming()
        {
            if (Settings.Connection.StubMessaging)
            {
                return;
            }

            var factory = new ConnectionFactory()
            {
                HostName = Settings.Connection.RabbitMQHostname,
                UserName = Settings.Connection.RabbitMQUsername,
                Password = Settings.Connection.RabbitMQPassword
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare(DeadletterExchangeName, "direct", true, false);
            channel.QueueDeclare(DeadletterQueueName, true, false, false);
            channel.QueueBind(DeadletterQueueName, DeadletterExchangeName, DeadletterRoutingKey);

            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("x-dead-letter-exchange", DeadletterExchangeName);
            args.Add("x-dead-letter-routing-key", DeadletterRoutingKey);

            channel.QueueDeclare(QueueName, true, false, false, args);

            consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (ch, ea) =>
            {
                if (ea.Body.IsEmpty)
                {
                    return;
                }

                T obj;

                try
                {
                    Logger.LogDebug($"[Q: {QueueName}] Handling message: {ea.DeliveryTag}");
                    var raw = Encoding.UTF8.GetString(ea.Body.ToArray());
                    obj = JsonConvert.DeserializeObject<T>(raw);
                    await HandleMessage(obj).ConfigureAwait(false);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[Q: {QueueName}] Exception occurred when handling message: {ea.DeliveryTag}: {ex}");
                    channel.BasicNack(ea.DeliveryTag, false, !ea.Redelivered);
                }
            };

            channel.BasicConsume(QueueName, false, consumer);
        }

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
    }
}
