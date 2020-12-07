using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Koasta.Shared.Configuration;
using RabbitMQ.Client;

namespace Koasta.Shared.Queueing
{
    internal class RabbitMQMessagePublisher : IMessagePublisher
    {
        private readonly ISettings settings;

        public RabbitMQMessagePublisher(ISettings settings)
        {
            this.settings = settings;
        }

        public void DirectPublish<T>(string queue, string deadletterRoutingKey, string deadletterExchange, string deadletterQueue, T data) where T : class
        {
            var factory = new ConnectionFactory()
            {
                HostName = settings.Connection.RabbitMQHostname,
                UserName = settings.Connection.RabbitMQUsername,
                Password = settings.Connection.RabbitMQPassword
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare(deadletterExchange, "direct", true, false);
            channel.QueueDeclare(deadletterQueue, true, false, false);
            channel.QueueBind(deadletterQueue, deadletterExchange, deadletterRoutingKey);

            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("x-dead-letter-exchange", deadletterExchange);
            args.Add("x-dead-letter-routing-key", deadletterRoutingKey);

            channel.QueueDeclare(queue, true, false, false, args);

            var serialisedData = JsonConvert.SerializeObject(data, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var body = Encoding.UTF8.GetBytes(serialisedData);

            channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: null, body: body);
        }

        public void Publish<T>(string exchange, PublishStrategy strategy, T data) where T : class
        {
            Publish<T>(exchange, strategy, $"{exchange}.publish", data);
        }

        public void Publish<T>(string exchange, PublishStrategy strategy, string routingKey, T data) where T : class
        {
            var factory = new ConnectionFactory()
            {
                HostName = settings.Connection.RabbitMQHostname,
                UserName = settings.Connection.RabbitMQUsername,
                Password = settings.Connection.RabbitMQPassword
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            if (strategy == PublishStrategy.RoundRobin)
            {
                channel.ExchangeDeclare(exchange, "x-consistent-hash", true, false);
            }
            else if (strategy == PublishStrategy.Fanout)
            {
                channel.ExchangeDeclare(exchange, "fanout", true, false);
            }
            else
            {
                throw new NotImplementedException();
            }

            var key = routingKey;
            if (strategy == PublishStrategy.RoundRobin)
            {
                key = $"{key}.{Guid.NewGuid()}";
            }

            var serialisedData = JsonConvert.SerializeObject(data, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var body = Encoding.UTF8.GetBytes(serialisedData);

            channel.BasicPublish(exchange: exchange, routingKey: key, basicProperties: null, body: body);
        }
    }
}
