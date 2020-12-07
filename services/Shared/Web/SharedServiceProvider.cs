using Microsoft.AspNetCore.Builder;
using Koasta.Shared.Middleware;
using Koasta.Shared.Web.Middleware;
using Koasta.Shared.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Koasta.Shared.Models;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace Koasta.Shared.Web
{
    public static class SharedServiceProvider
    {
        public static void UseSharedServices(this IApplicationBuilder app)
        {
            if (app == null)
            {
                return;
            }

            var loggerFactory = app.ApplicationServices.GetService(typeof(ILoggerFactory)) as ILoggerFactory;
            var config = app.ApplicationServices.GetService(typeof(ISettings)) as ISettings;
            var logger = loggerFactory.CreateLogger("Startup");
            var pathBase = "";
            var environment = app.ApplicationServices.GetService(typeof(IEnvironment)) as IEnvironment;

            if (environment.IsProduction)
            {
                if (!string.IsNullOrWhiteSpace(config.Meta.PathBase))
                {
                    logger.LogDebug("Setting base path to {0}", config.Meta.PathBase);
                    pathBase = config.Meta.PathBase;
                    app.UsePathBase(pathBase);
                }
                else
                {
                    logger.LogDebug("Skipping setting base path as no value was provided");
                }
            }
            else
            {
                logger.LogDebug("Skipping setting base path as no configuration parameter was found");
            }

            app.UseMiddleware<CorsMiddleware>();
            app.UseMiddleware<ExceptionMiddleware>();
            if (!environment.IsProduction)
            {
                var scheme = environment.IsDevelopment ? "http" : "https";
                app.UseSwagger(c =>
                {
                    c.RouteTemplate = "swagger/{documentName}/swagger.json";
                    c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Servers = new System.Collections.Generic.List<OpenApiServer>
                    {
                        new OpenApiServer { Url = $"{scheme}://{httpReq.Host.Value}{pathBase}" }
                    });
                });
                app.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "API V1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());

            app.UseHealthChecks("/ready", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var response = new HealthCheckReponse
                    {
                        Status = report.Status.ToString(),
                        HealthChecks = report.Entries.Select(x => new IndividualHealthCheckResponse
                        {
                            Component = x.Key,
                            Status = x.Value.Status.ToString(),
                            Description = x.Value.Description
                        }),
                        HealthCheckDuration = report.TotalDuration
                    };
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response)).ConfigureAwait(false);
                }
            });

            if (!string.IsNullOrWhiteSpace(config.Connection.MemcachedUrl))
            {
                app.UseEnyimMemcached();
            }
        }
    }
}
