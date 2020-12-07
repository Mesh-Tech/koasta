using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Koasta.Shared.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Koasta.Service.EventService.Consumers
{
    public abstract class AbstractExchangeConsumer<T> : IConsumer, IDisposable where T : class
    {
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;

        protected abstract string ExchangeName { get; }
        protected abstract string ExchangeType { get; }
        protected abstract string QueueName { get; }
        protected abstract string RoutingKey { get; }
        protected abstract ISettings Settings { get; }

        protected abstract void HandleMessage(T message);

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

                T obj;

                try
                {
                    var raw = Encoding.UTF8.GetString(ea.Body.ToArray());
                    obj = JsonConvert.DeserializeObject<T>(raw);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return;
                }

                HandleMessage(obj);
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
                        channel.BasicCancel(consumer.ConsumerTags.FirstOrDefault());
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
