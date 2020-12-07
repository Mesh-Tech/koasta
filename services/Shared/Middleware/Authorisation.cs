using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Koasta.Shared.Types;
using Koasta.Shared.Repository;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Koasta.Shared.Middleware
{
    public class AuthorisationMiddleware : IAsyncActionFilter
    {
        private readonly EmployeeSessionRepository employeeSessions;
        private readonly UserSessionRepository userSessions;
        private readonly EmployeeRoleRepository employeeRoles;
        private readonly TokenRepository _tokenRepository;
        private readonly ISettings settings;
        private readonly IDistributedCache cache;

        private enum AuthorisationResult
        {
            AbortUnauthorised,
            InjectAndContinue,
            Continue
        }

        public AuthorisationMiddleware(EmployeeSessionRepository employeeSessions,
                                       UserSessionRepository userSessions,
                                       EmployeeRoleRepository employeeRoles,
                                       TokenRepository tokenRepository,
                                       IDistributedCache cache,
                                       ISettings settings)
        {
            this.employeeSessions = employeeSessions;
            this.userSessions = userSessions;
            this.employeeRoles = employeeRoles;
            this._tokenRepository = tokenRepository;
            this.settings = settings;
            this.cache = cache;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.Request.Path.StartsWithSegments(new PathString("/swagger"), StringComparison.InvariantCultureIgnoreCase))
            {
                await next().ConfigureAwait(false);
                return;
            }

            var routeData = context.RouteData;
            var authContext = DetermineAuthContext(context);
            var userType = authContext.UserType;

            if (!await ValidateEmployeeApiKey(context, authContext).ConfigureAwait(false))
            {
                AbortUnauthorised(context);
                return;
            }

            switch (ExtractAuthorisationToken(context, authContext))
            {
                case AuthorisationResult.AbortUnauthorised:
                    AbortUnauthorised(context);
                    return;
                case AuthorisationResult.InjectAndContinue:
                    await InjectContext(context, next, authContext).ConfigureAwait(false);
                    return;
            }

            switch (await EnforceRBACRequirements(authContext, routeData).ConfigureAwait(false))
            {
                case AuthorisationResult.AbortUnauthorised:
                    AbortUnauthorised(context);
                    return;
                case AuthorisationResult.InjectAndContinue:
                    await InjectContext(context, next, authContext).ConfigureAwait(false);
                    return;
            }

            await InjectContext(context, next, authContext).ConfigureAwait(false);
        }

        private void AbortUnauthorised(ActionExecutingContext context)
        {
            context.HttpContext.Response.Body = Stream.Null;
            context.HttpContext.Response.StatusCode = 401;
        }

        private async Task InjectContext(ActionExecutingContext context, ActionExecutionDelegate next, AuthContext authContext)
        {
            context.HttpContext.Items.Add("pubcrawlauthcontext", authContext);
            await next().ConfigureAwait(false);
        }

        private bool RouteRequiresAtLeastOnePermission(ApiAuthRequirement requirements)
        {
            return requirements.AdministerCompany
              || requirements.AdministerSystem
              || requirements.AdministerVenue
              || requirements.WorkWithCompany
              || requirements.WorkWithVenue;
        }

        private bool RoleHasCorrectPermissions(EmployeeRole role, ApiAuthRequirement requirements)
        {
            if (requirements.AdministerCompany && !role.CanAdministerCompany)
            {
                return false;
            }

            if (requirements.AdministerVenue && !role.CanAdministerVenue)
            {
                return false;
            }

            if (requirements.WorkWithCompany && !role.CanWorkWithCompany)
            {
                return false;
            }

            if (requirements.WorkWithVenue && !role.CanWorkWithVenue)
            {
                return false;
            }

            if (requirements.AdministerSystem && !role.CanAdministerSystem)
            {
                return false;
            }

            return true;
        }

        private AuthContext DetermineAuthContext(ActionExecutingContext context)
        {
            var authContext = new AuthContext();

            // Determine what kind of user we're working with. Employees require passwords whereas Users require verification tokens.
            UserType userType;
            StringValues userAgentHeaderValues;
            if (!context.HttpContext.Request.Headers.TryGetValue("User-Agent", out userAgentHeaderValues) || userAgentHeaderValues.Count == 0)
            {
                userType = UserType.Employee;
            }
            else if ("pubcrawl/ios".Equals(userAgentHeaderValues[0], StringComparison.OrdinalIgnoreCase))
            {
                userType = UserType.User;
            }
            else if ("pubcrawl/android".Equals(userAgentHeaderValues[0], StringComparison.OrdinalIgnoreCase))
            {
                userType = UserType.User;
            }
            else
            {
                userType = UserType.Employee;
            }

            authContext.UserType = userType;

            return authContext;
        }

        private async Task<bool> ValidateEmployeeApiKey(ActionExecutingContext context, AuthContext authContext)
        {
            var userType = authContext.UserType;

            // Parse out the API Key
            if (userType != UserType.Employee)
            {
                return true;
            }

            var apiKeyHeaderValue = context.HttpContext.Request.Headers.TryGetValue("x-api-key", out StringValues key) ? key : StringValues.Empty;
            if (apiKeyHeaderValue.Count == 0)
            {
                return false;
            }

            var parsedKey = apiKeyHeaderValue[0];

            if (string.IsNullOrWhiteSpace(parsedKey))
            {
                return false;
            }

            var cachedToken = await cache.GetStringAsync($"apik-{parsedKey}").ConfigureAwait(false);
            var tokenCached = false;
            ApiToken storedToken = null;
            if (!string.IsNullOrWhiteSpace(cachedToken))
            {
                try
                {
                    storedToken = JsonConvert.DeserializeObject<ApiToken>(cachedToken);
                    tokenCached = true;
                }
                catch { }
            }

            if (storedToken == null)
            {
                var token = (await _tokenRepository.GetApiAuthToken(parsedKey).ConfigureAwait(false))
                .Ensure(t => t.HasValue, "Token found in database")
                .OnSuccess(t => t.Value);

                if (token.IsSuccess)
                {
                    storedToken = token.Value;
                }
            }

            if (storedToken == null || !parsedKey.Equals(storedToken.ApiTokenValue, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!tokenCached)
            {
                await cache.SetStringAsync(
                    $"apik-{storedToken.ApiTokenValue}",
                    JsonConvert.SerializeObject(storedToken),
                    new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(5)
                    }
                ).ConfigureAwait(false);
            }

            return true;
        }

        private AuthorisationResult ExtractAuthorisationToken(ActionExecutingContext context, AuthContext authContext)
        {
            var routeData = context.RouteData;

            // Grab the authorization header. It's a realistic case that this could be missing as not all of our APIs are authenticated, so we can't assume it exists.
            StringValues authHeaderValues;
            if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out authHeaderValues) || authHeaderValues.Count == 0)
            {
                authContext.AuthHeader = Maybe<String>.None;
                authContext.AuthToken = Maybe<String>.None;

                // Grab the route name via the [ActionName(...)] annotation on a controller method.
                // If you don't annotate your methods, or have not configured this route as authenticated, we won't apply any auth restrictions.
                var route = (string)routeData.Values["action"];

                if (route == null || !settings.Meta.AuthRequirements.ContainsKey(route) || !RouteRequiresAtLeastOnePermission(settings.Meta.AuthRequirements[route]))
                {
                    return AuthorisationResult.InjectAndContinue;
                }
                else
                {
                    return AuthorisationResult.AbortUnauthorised;
                }
            }

            // We need to be careful here when parsing the header, as it's completely unstructured and could be filled with garbage data.
            var authHeader = authHeaderValues[0];
            var authTokenComponents = authHeader.Split(" ");
            string authToken = null;

            authContext.AuthHeader = Maybe<string>.From(authHeader);
            if (authTokenComponents.Length > 1 && "Bearer".Equals(authTokenComponents[0], StringComparison.OrdinalIgnoreCase))
            {
                authToken = authTokenComponents[1];
            }

            authContext.AuthToken = Maybe<string>.From(authToken);

            StringValues authTypeHeaderValues;
            if (context.HttpContext.Request.Headers.TryGetValue("x-koasta-authtype", out authTypeHeaderValues))
            {
                var authTypeHeader = authTypeHeaderValues[0];
                authContext.AuthType = authTypeHeader.ToAuthType();
            }

            return AuthorisationResult.Continue;
        }

        private async Task<AuthorisationResult> EnforceRBACRequirements(AuthContext authContext, Microsoft.AspNetCore.Routing.RouteData routeData)
        {
            var userType = authContext.UserType;

            // Grab the route name via the [ActionName(...)] annotation on a controller method.
            // If you don't annotate your methods, or have not configured this route as authenticated, we won't apply any auth restrictions.
            var routeName = (string)routeData.Values["action"];

            if (routeName == null || !settings.Meta.AuthRequirements.ContainsKey(routeName))
            {
                return AuthorisationResult.InjectAndContinue;
            }

            var requirements = settings.Meta.AuthRequirements[routeName];

            Maybe<Employee> employee = null;
            Maybe<EmployeeRole> employeeRole = null;
            Maybe<User> user = null;

            if (userType != settings.Meta.AuthRequirements[routeName].UserType && UserType.Any != settings.Meta.AuthRequirements[routeName].UserType)
            {
                return AuthorisationResult.AbortUnauthorised;
            }

            if (authContext.AuthToken.HasNoValue)
            {
                return AuthorisationResult.AbortUnauthorised;
            }

            var authToken = authContext.AuthToken.Value;

            // Depending on the session type, attempt to fetch the equivalent model (User or Employee).
            if (userType == UserType.Employee)
            {
                employee = (await employeeSessions.FetchEmployeeSessionAndEmployeeFromToken(authToken).ConfigureAwait(false))
                  .OnBoth(x => x.IsSuccess
                    ? x.Value
                    : Maybe<Employee>.None
                  );

                if (employee.HasNoValue)
                {
                    return AuthorisationResult.AbortUnauthorised;
                }

                // For employees, their permissions are scoped to an employee role. First we fetch the role.
                employeeRole = (await employeeRoles.FetchEmployeeRole(employee.Value.RoleId).ConfigureAwait(false))
                  .OnBoth(x => x.IsSuccess
                    ? x.Value
                    : Maybe<EmployeeRole>.None
                  );

                // If the employee role wasn't found but the route didn't require any permissions, we let them through.
                if (employeeRole.HasNoValue && RouteRequiresAtLeastOnePermission(requirements))
                {
                    return AuthorisationResult.AbortUnauthorised;
                }

                // Verify each of the role permissions in turn, if one is missing and the route requires it, deny access.
                if (!RoleHasCorrectPermissions(employeeRole.Value, requirements))
                {
                    return AuthorisationResult.AbortUnauthorised;
                }
            }
            else if (userType == UserType.User)
            {
                user = (await userSessions.FetchUserSessionAndUserFromToken(authToken, (int)authContext.AuthType).ConfigureAwait(false))
                  .OnBoth(x => x.IsSuccess
                    ? x.Value
                    : Maybe<User>.None
                  );

                if (user.HasNoValue)
                {
                    return AuthorisationResult.AbortUnauthorised;
                }
            }

            // After verifying the user's permissions, update the context with relevant user data and continue.
            authContext.Employee = employee;
            authContext.EmployeeRole = employeeRole;
            authContext.User = user;

            return AuthorisationResult.Continue;
        }
    }
}
