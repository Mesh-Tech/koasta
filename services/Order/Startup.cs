using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Koasta.Shared.DI;
using Koasta.Shared.Web;
using Koasta.Service.OrderService.Utils;

namespace Koasta.Service.OrderService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddShared<Startup>();
            services.AddSingleton<IOrderCalculator, OrderCalculator>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSharedServices();
        }
    }
}
