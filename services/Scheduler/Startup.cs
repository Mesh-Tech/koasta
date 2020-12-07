using Hangfire;
using Hangfire.MemoryStorage;
using Koasta.Service.Scheduler.Jobs;
using Koasta.Shared.DI;
using Koasta.Shared.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Koasta.Service.Scheduler
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddShared<Startup>();

            services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseDefaultTypeSerializer()
                .UseMemoryStorage();
            });

            services.AddHangfireServer();
            services.AddSingleton<ITokenRefreshJob, TokenRefreshJob>();
        }

        public void Configure(
            IApplicationBuilder app, 
            IRecurringJobManager recurringJobManager,
            IServiceProvider serviceProvider)
        {
            app.UseSharedServices();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new AllowAnyAuthorizationFilter() }
            });

            var tokenJob = serviceProvider.GetService<ITokenRefreshJob>();

            recurringJobManager.AddOrUpdate(
                tokenJob.Name,
                () => tokenJob.ProcessJob(),
                tokenJob.TriggerTime);
        }
    }
}
