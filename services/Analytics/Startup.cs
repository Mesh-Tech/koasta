using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Koasta.Shared.DI;
using Koasta.Shared.Web;

namespace Koasta.Service.AnalyticsService
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddShared<Startup>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSharedServices();
        }
    }
}
