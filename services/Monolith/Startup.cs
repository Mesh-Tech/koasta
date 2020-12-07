using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Koasta.Service.Auth.Utils;
using Koasta.Shared.DI;
using Koasta.Shared.Web;
using Koasta.Service.Auth.Checks;
using Koasta.Service.OrderService.Utils;
using System;
using Koasta.Service.EventService.Middleware;

namespace Koasta.Service.MonolithService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddShared<Startup>();
            services.AddSingleton(typeof(WebRequestHelper));
            services.AddSingleton(typeof(TokenUtil));
            services.AddSingleton(typeof(AppleJWTKeyRefresher));
            services.AddSingleton(typeof(GoogleJWTKeyRefresher));
            services.AddSingleton<IOrderCalculator, OrderCalculator>();

            services.AddMvcCore().AddNewtonsoftJson();
            services.AddHealthChecks()
                .AddCheck<AppleIdHealthCheck>("appleId")
                .AddCheck<GoogleSignInHealthCheck>("googleSignIn");

            services.AddMvc()
              .AddApplicationPart(typeof(Auth.Startup).Assembly)
              .AddApplicationPart(typeof(AnalyticsService.Startup).Assembly)
              .AddApplicationPart(typeof(CompanyService.Startup).Assembly)
              .AddApplicationPart(typeof(EmployeeService.Startup).Assembly)
              .AddApplicationPart(typeof(ImageService.Startup).Assembly)
              .AddApplicationPart(typeof(MenuService.Startup).Assembly)
              .AddApplicationPart(typeof(OrderService.Startup).Assembly)
              .AddApplicationPart(typeof(ProductService.Startup).Assembly)
              .AddApplicationPart(typeof(SubscriptionService.Startup).Assembly)
              .AddApplicationPart(typeof(TagService.Startup).Assembly)
              .AddApplicationPart(typeof(UserService.Startup).Assembly)
              .AddApplicationPart(typeof(VenueService.Startup).Assembly)
              .AddApplicationPart(typeof(FlagService.Startup).Assembly);
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

            var refresher = app.ApplicationServices.GetService<AppleJWTKeyRefresher>();
            refresher.Start();

            var refresher2 = app.ApplicationServices.GetService<GoogleJWTKeyRefresher>();
            refresher2.Start();
        }
    }
}
