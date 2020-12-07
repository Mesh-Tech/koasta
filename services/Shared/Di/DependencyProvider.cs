using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Koasta.Shared.Billing;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Queueing;
using Koasta.Shared.Types;
using Koasta.Shared.Web;
using Koasta.Shared.Repository;
using System.Web;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.DataProtection;
using System;
using Koasta.Shared.Crypto;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;

namespace Koasta.Shared.DI
{
    public static class DependencyProvider
    {
        public static void AddSharedCore<TS>(this IServiceCollection services, bool addUIServices = false)
        {
            var settings = SettingsProvider.LoadSettings();

            var environment = System.Environment.GetEnvironmentVariable("PUBCRAWL_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environment))
            {
                environment = "default";
            }

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.AddLogging(configure => {
                configure.AddConsole();
                if (!environment.Equals("default", StringComparison.OrdinalIgnoreCase))
                {
                    // We use the log path /var/log/koasta-{environment}.log on Linux, as we use a Linux-based AMI in AWS which
                    // automatically publishes logs to CloudWatch on our behalf at this file location.
                    var logPath = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? $"/var/log/koasta-{environment}.log" : Path.Join(System.Environment.CurrentDirectory, $"koasta-{environment}.log");
                    configure.AddFile(logPath, fileOptions => {
                        fileOptions.Append = true;
                        fileOptions.FileSizeLimitBytes = 1024 * 1024 * 10; // 10 megabytes
                        fileOptions.MaxRollingFiles = 5;
                    });
                }
            })
              .Configure<LoggerFilterOptions>(options => options.MinLevel = settings.Meta.Debug ? LogLevel.Debug : LogLevel.Information);
            services.AddMvc();
            services.AddSingleton(typeof(ISettings), settings);
            services.AddSingleton<IEnvironment, Koasta.Shared.Configuration.Environment>();
            services.AddSingleton(typeof(CompanyRepository));
            services.AddSingleton(typeof(EmployeeRepository));
            services.AddSingleton(typeof(EmployeeRoleRepository));
            services.AddSingleton(typeof(EmployeeSessionRepository));
            services.AddSingleton(typeof(UserRepository));
            services.AddSingleton(typeof(UserSessionRepository));
            services.AddSingleton(typeof(MenuRepository));
            services.AddSingleton(typeof(MenuItemRepository));
            services.AddSingleton(typeof(OrderRepository));
            services.AddSingleton(typeof(OrderLineRepository));
            services.AddSingleton(typeof(ProductRepository));
            services.AddSingleton(typeof(ProductTypeRepository));
            services.AddSingleton(typeof(VenueRepository));
            services.AddSingleton(typeof(ReviewRepository));
            services.AddSingleton(typeof(SubscriptionPackageRepository));
            services.AddSingleton(typeof(SubscriptionPlanRepository));
            services.AddSingleton(typeof(ImageRepository));
            services.AddSingleton(typeof(TagRepository));
            services.AddSingleton(typeof(DeviceRepository));
            services.AddSingleton(typeof(TokenRepository));
            services.AddSingleton(typeof(MediaRepository));
            services.AddSingleton(typeof(DocumentRepository));
            services.AddSingleton(typeof(VenueDocumentRepository));
            services.AddSingleton(typeof(FeatureFlagRepository));
            services.AddSingleton(typeof(VenueOpeningTimeRepository));
            services.AddSingleton<IKeyStoreHelper, KeyStoreHelper>();
            if (settings.Connection.StubMessaging)
            {
                services.AddSingleton(typeof(IMessagePublisher), typeof(StubMessagePublisher));
            }
            else
            {
                services.AddSingleton(typeof(IMessagePublisher), typeof(RabbitMQMessagePublisher));
            }
            if (settings.Connection.StubBilling)
            {
                services.AddSingleton(typeof(IBillingManager), typeof(StubBillingManager));
            }
            else
            {
                services.AddSingleton(typeof(IBillingManager), typeof(BillingManager));
            }
            services.AddSingleton(typeof(Constants));
            services.AddSingleton(typeof(AnalyticsRepository));
            services.AddSingleton<ICryptoHelper, CryptoHelper>();

            var startupType = new StartupType
            {
                Type = typeof(TS)
            };

            services.AddSingleton(typeof(StartupType), startupType);

            services.AddHealthChecks().AddNpgSql(settings.Connection.DatabaseConnectionString);

            if (settings.Meta.RequiresMessageQueue && !settings.Connection.StubMessaging)
            {
                string rabbitmqconnection = $"amqp://{HttpUtility.UrlEncode(settings.Connection.RabbitMQUsername)}:{ HttpUtility.UrlEncode(settings.Connection.RabbitMQPassword)}@{settings.Connection.RabbitMQHostname}:5672";

                services
                .AddHealthChecks()
                .AddRabbitMQ(rabbitConnectionString: rabbitmqconnection);
            }

            if (addUIServices && !environment.Equals("default", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDataProtection()
                    .SetApplicationName($"koasta{environment}")
                    .PersistKeysToAWSSystemsManager("/Koasta/DataProtection");
            }

            if (!string.IsNullOrWhiteSpace(settings.Connection.MemcachedUrl))
            {
                services.AddEnyimMemcached(options => options.AddServer(settings.Connection.MemcachedUrl, 11211));
            }
        }

        public static void AddShared<TS>(this IServiceCollection services)
        {
            AddSharedCore<TS>(services);
            var settings = SettingsProvider.LoadSettings();

            services.AddMvc(options =>
            {
                if (settings.Meta.Debug)
                {
                    options.Filters.Add(new BadRequestDebugMiddleware());
                }
            });

            services.AddSingleton(typeof(ApiWarmer));
            services.AddScoped<AuthorisationMiddleware>();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Koasta API", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Employee Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityDefinition("API Key", new OpenApiSecurityScheme
                {
                    Description = "Employee API key",
                    In = ParameterLocation.Header,
                    Name = "x-api-key",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    },
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "API Key"
                            },
                            Scheme = "oauth2",
                            Name = "API Key",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });
        }
    }
}
