using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Koasta.Shared.DI;
using Koasta.Shared.Web;
using Koasta.Service.Auth.Utils;
using Koasta.Service.Auth.Checks;

namespace Koasta.Service.Auth
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddShared<Startup>();
            services.AddSingleton(typeof(TokenUtil));
            services.AddSingleton(typeof(WebRequestHelper));
            services.AddSingleton(typeof(AppleJWTKeyRefresher));
            services.AddSingleton(typeof(GoogleJWTKeyRefresher));

            services.AddMvcCore().AddNewtonsoftJson();
            services.AddHealthChecks()
                .AddCheck<AppleIdHealthCheck>("appleId")
                .AddCheck<GoogleSignInHealthCheck>("googleSignIn");
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSharedServices();

            var refresher = app.ApplicationServices.GetService<AppleJWTKeyRefresher>();
            refresher.Start();

            var refresher2 = app.ApplicationServices.GetService<GoogleJWTKeyRefresher>();
            refresher2.Start();
        }
    }
}
