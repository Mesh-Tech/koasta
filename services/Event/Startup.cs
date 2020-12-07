using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Koasta.Shared.DI;
using Koasta.Shared.Web;
using System.Collections.Generic;
using System;
using System.Linq;
using Koasta.Service.EventService.Consumers;
using Koasta.Service.EventService.Middleware;

namespace Koasta.Service.EventService
{
    public class Startup
    {
        private List<Type> consumerTypes;
        private List<IConsumer> consumers;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddShared<Startup>();
            consumerTypes = QueryConsumerTypes();
            consumerTypes.ForEach(consumer => services.AddSingleton(consumer));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSharedServices();
            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(10),
                ReceiveBufferSize = 4 * 1024
            };
            app.UseWebSockets(webSocketOptions);
            app.UseMiddleware<WebsocketMiddleware>();

            consumers = consumerTypes
              .Select(consumerType => app.ApplicationServices.GetService(consumerType))
              .Select(consumer => (IConsumer)consumer)
              .ToList();

            consumers.ForEach(consumer => consumer.StartConsuming());
        }

        private List<Type> QueryConsumerTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
              .Where(x => typeof(IConsumer).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract && !x.Name.Equals("AbstractExchangeConsumer", StringComparison.Ordinal))
              .ToList();
        }
    }
}
