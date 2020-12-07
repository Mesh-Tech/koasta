using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Koasta.Shared.DI;
using Koasta.Shared.Models;
using Koasta.Service.Admin.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.UI.Services;
using Koasta.Shared.Configuration;
using Koasta.Service.Admin.Middleware;
using Westwind.AspNetCore.LiveReload;
using System;

namespace Koasta.Service.Admin
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var isDevelopment = (System.Environment.GetEnvironmentVariable("PUBCRAWL_ENVIRONMENT") ?? "default").Equals("default", StringComparison.OrdinalIgnoreCase);
            if (isDevelopment)
            {
                services.AddLiveReload();
            }

            services.AddSharedCore<Startup>(true);
            services.AddSingleton<IWebRequestHelper, WebRequestHelper>();
            services.AddSingleton<ICoordinatesService, CoordinatesService>();
            services.AddSingleton<IFeedService, FeedService>();
            services.AddScoped<IUserStore<Employee>, WebUserStore>();
            services.AddScoped<IRoleStore<EmployeeRole>, WebRoleStore>();
            services.AddScoped<IPasswordHasher<Employee>, WebUserStore>();
            services.AddScoped<IPasswordValidator<Employee>, PasswordDictionaryChecker>();
            services.AddTransient<IEmailSender, MailjetEmailSender>();
            services.ConfigureApplicationCookie(options => {
                options.LoginPath = options.LoginPath.ToString().ToLowerInvariant();
                options.AccessDeniedPath = "/";
                options.LogoutPath = options.LogoutPath.ToString().ToLowerInvariant();
                options.ReturnUrlParameter = options.ReturnUrlParameter.ToLowerInvariant();
            });
            services.AddDefaultIdentity<Employee>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 10;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
                .AddRoles<EmployeeRole>()
                .AddUserStore<WebUserStore>()
                .AddRoleStore<WebRoleStore>();
            services.AddRazorPages(options => {
                options.Conventions.AuthorizeFolder("/", "RequireCompanyRole")
                    .AllowAnonymousToPage("/Index")
                    .AllowAnonymousToPage("/Login")
                    .AllowAnonymousToPage("/Register")
                    .AllowAnonymousToPage("/Lockout")
                    .AllowAnonymousToPage("/ForgotPassword")
                    .AllowAnonymousToPage("/ForgotPasswordConfirmation")
                    .AllowAnonymousToPage("/AccessDenied")
                    .AllowAnonymousToPage("/RegisterConfirmation")
                    .AllowAnonymousToPage("/ResendEmailConfirmation");
                options.Conventions.AuthorizeFolder("/Admin", "RequireSysadminRole");
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireCompanyRole", policy => policy.RequireRole(new List<string> { "Bar Staff", "Bar Manager", "Company Staff", "Company Manager", "Sysadmin" }));
                options.AddPolicy("RequireSysadminRole", policy => policy.RequireRole("Sysadmin"));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IEnvironment environment)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseLiveReload();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            if (!environment.IsDevelopment)
            {
                app.UseMiddleware<ForceHTTPSMiddleware>();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}
