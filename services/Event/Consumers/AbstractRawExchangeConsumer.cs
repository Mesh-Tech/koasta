using System;
using System.Text;
using Koasta.Shared.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Koasta.Service.EventService.Consumers
{
    public abstract class AbstractRawExchangeConsumer : IConsumer, IDisposable
    {
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;

        protected abstract string ExchangeName { get; }
        protected abstract string ExchangeType { get; }
        protected abstract string QueueName { get; }
        protected abstract string RoutingKey { get; }
        protected abstract ISettings Settings { get; }

        protected abstract void HandleMessage(string message);

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
    }
}
