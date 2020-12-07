using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Koasta.Service.Auth.Models;
using Koasta.Service.Auth.Utils;
using Koasta.Shared.Billing;
using Koasta.Shared.Configuration;
using Koasta.Shared.Database;
using Koasta.Shared.Middleware;
using Koasta.Shared.Models;
using Koasta.Shared.PatchModels;
using Koasta.Shared.Types;
using Koasta.Shared.Crypto;
using Koasta.Services.Auth.Models;
using CSharpFunctionalExtensions;

namespace Koasta.Service.Auth.Controllers
{
    [ServiceFilter(typeof(AuthorisationMiddleware))]
    [Route("/auth")]
    public partial class AuthoriseController : Controller
    {
        private readonly EmployeeRepository employees;
        private readonly EmployeeSessionRepository employeeSessions;
        private readonly UserRepository users;
        private readonly UserSessionRepository userSessions;
        private readonly TokenUtil tokenUtil;
        private readonly ISettings settings;
        private readonly IBillingManager billingManager;
        private readonly ICryptoHelper crypto;

        public AuthoriseController(EmployeeRepository employees,
                                   EmployeeSessionRepository employeeSessions, UserRepository users,
                                   UserSessionRepository userSessions, TokenUtil tokenUtil, ISettings settings,
                                   IBillingManager billingManager, ICryptoHelper crypto)
        {
            this.employees = employees;
            this.employeeSessions = employeeSessions;
            this.users = users;
            this.userSessions = userSessions;
            this.tokenUtil = tokenUtil;
            this.settings = settings;
            this.billingManager = billingManager;
            this.crypto = crypto;
        }

        [HttpPost]
        [Route("authorise")]
        [ActionName("authorise")]
        [ProducesResponseType(typeof(EmployeeSession), 200)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Authorise([FromBody] DtoAuthoriseRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var auth = this.GetAuthContext();

            if (auth.UserType == UserType.Employee)
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest();
                }

                return await AuthoriseEmployee(request.Username, request.Password).ConfigureAwait(false);
            }
            else if (auth.AuthToken.HasNoValue || auth.AuthType == AuthType.Unknown)
            {
                return BadRequest();
            }
            else
            {
                return await AuthoriseUser(auth.AuthToken.Value, auth.AuthType, request.FirstName, request.LastName).ConfigureAwait(false);
            }
        }

        [HttpPost]
        [Route("refresh")]
        [ActionName("refresh")]
        [ProducesResponseType(typeof(EmployeeSession), 200)]
        public async Task<IActionResult> Refresh([FromBody] DtoRefreshRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var auth = this.GetAuthContext();

            var currentSession = await employeeSessions.FetchEmployeeSessionFromTokens(auth.AuthToken.Value, request.RefreshToken)
                .Ensure(s => s.HasValue, "Session found")
                .OnSuccess(s => s.Value)
                .ConfigureAwait(false);

            if (!currentSession.IsSuccess)
            {
                return Unauthorized();
            }

            var sessionToken = tokenUtil.GenerateToken();
            var refreshToken = tokenUtil.GenerateToken();

            var session = new EmployeeSession
            {
                AuthToken = sessionToken,
                RefreshToken = refreshToken,
                Expiry = DateTime.UtcNow.AddMinutes(settings.Auth.AuthTokenValidityMinutes),
                RefreshExpiry = DateTime.UtcNow.AddMinutes(settings.Auth.RefreshTokenValidityMinutes),
                EmployeeId = auth.Employee.Value.EmployeeId
            };

            var sessionId = await employeeSessions.CreateEmployeeSession(session).ConfigureAwait(false);
            if (sessionId.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok(new DtoAuthoriseEmployeeResponse
            {
                AuthToken = session.AuthToken,
                RefreshToken = session.RefreshToken,
                Expiry = session.Expiry,
                RefreshExpiry = session.RefreshExpiry
            });
        }

        private async Task<IActionResult> AuthoriseEmployee(string username, string password)
        {
            var employeeResult = await employees.FetchEmployeeByUsername(username).ConfigureAwait(false);
            if (employeeResult.IsFailure)
            {
                return StatusCode(500);
            }

            var employee = employeeResult.Value;
            if (employee.HasNoValue)
            {
                return Unauthorized();
            }

            if (!crypto.IsValid(password, employee.Value.PasswordHash))
            {
                return Unauthorized();
            }

            var sessionToken = tokenUtil.GenerateToken();
            var refreshToken = tokenUtil.GenerateToken();

            var session = new EmployeeSession
            {
                AuthToken = sessionToken,
                RefreshToken = refreshToken,
                Expiry = DateTime.UtcNow.AddMinutes(settings.Auth.AuthTokenValidityMinutes),
                RefreshExpiry = DateTime.UtcNow.AddMinutes(settings.Auth.RefreshTokenValidityMinutes),
                EmployeeId = employee.Value.EmployeeId
            };

            var sessionId = await employeeSessions.CreateEmployeeSession(session).ConfigureAwait(false);
            if (sessionId.IsFailure)
            {
                return StatusCode(500);
            }

            return Ok(new DtoAuthoriseEmployeeResponse
            {
                AuthToken = session.AuthToken,
                RefreshToken = session.RefreshToken,
                Expiry = session.Expiry,
                RefreshExpiry = session.RefreshExpiry
            });
        }

        private async Task<IActionResult> AuthoriseUser(string authToken, AuthType authType, string firstName, string lastName)
        {
            if (authType == AuthType.Unknown)
            {
                return BadRequest();
            }

            var metadata = await tokenUtil.GetMetadataForToken(authToken, authType).ConfigureAwait(false);
            if (metadata == null || string.IsNullOrWhiteSpace(metadata.AuthIdentifier))
            {
                return BadRequest();
            }

            var user = await users.FetchUserForAuthIdentifier(metadata.AuthIdentifier, authType).ConfigureAwait(false);

            if (user.IsFailure)
            {
                return StatusCode(500);
            }

            if (user.Value.HasValue)
            {
                var userId = user.Value.Value.UserId;

                if (string.IsNullOrWhiteSpace(user.Value.Value.ExternalPaymentProcessorId))
                {
                    try
                    {
                        var billingId = await billingManager.CreateCustomer(user.Value.Value).ConfigureAwait(false);
                        await users.UpdateUser(new UserPatch
                        {
                            ResourceId = userId,
                            ExternalPaymentProcessorId = new PatchOperation<string> { Operation = OperationKind.Update, Value = billingId }
                        }).ConfigureAwait(false);
                    }
                    catch (Exception)
                    {
                        return StatusCode(500);
                    }
                }

                if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
                {
                    await users.UpdateUser(new UserPatch
                    {
                        ResourceId = userId,
                        FirstName = new PatchOperation<string> { Operation = OperationKind.Update, Value = firstName ?? "Anonymous" },
                        LastName = new PatchOperation<string> { Operation = OperationKind.Update, Value = lastName },
                    }).ConfigureAwait(false);
                }

                var existingSession = await userSessions.FetchUserSessionAndUserFromToken(authToken, (int)authType)
                    .Ensure(s => s.HasValue, "Data found")
                    .OnSuccess(s => s.Value)
                    .ConfigureAwait(false);

                if (existingSession.IsFailure)
                {
                    var session = new UserSession
                    {
                        AuthToken = authToken,
                        UserId = userId,
                        TokenType = (int)authType,
                        AuthTokenExpiry = metadata.Expiry
                    };

                    var sessionId = await userSessions.CreateUserSession(session).ConfigureAwait(false);
                    if (sessionId.IsFailure)
                    {
                        return StatusCode(500);
                    }
                }
            }
            else
            {
                var newUser = new User { IsVerified = true, WantAdvertising = false, RegistrationId = tokenUtil.GenerateToken(), FirstName = firstName ?? "Anonymous", LastName = lastName };
                switch (authType)
                {
                    case AuthType.Apple:
                        newUser.AppleUserIdentifier = metadata.AuthIdentifier;
                        break;
                    case AuthType.Google:
                        newUser.GoogleUserIdentifier = metadata.AuthIdentifier;
                        break;
                    case AuthType.Facebook:
                        newUser.FacebookUserIdentifier = metadata.AuthIdentifier;
                        break;
                    case AuthType.Unknown:
                        break;
                }
                var result = await users.CreateUser(newUser).ConfigureAwait(false);

                if (result.IsFailure)
                {
                    return StatusCode(500);
                }

                var userId = result.Value.Value;

                try
                {
                    var billingId = await billingManager.CreateCustomer(newUser).ConfigureAwait(false);
                    await users.UpdateUser(new UserPatch
                    {
                        ResourceId = userId,
                        ExternalPaymentProcessorId = new PatchOperation<string> { Operation = OperationKind.Update, Value = billingId }
                    }).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    return StatusCode(500);
                }

                var existingSession = await userSessions.FetchUserSessionAndUserFromToken(authToken, (int)authType)
                    .Ensure(s => s.HasValue, "Data found")
                    .OnSuccess(s => s.Value)
                    .ConfigureAwait(false);

                if (existingSession.IsFailure)
                {
                    var session = new UserSession
                    {
                        AuthToken = authToken,
                        UserId = userId,
                        TokenType = (int)authType,
                        AuthTokenExpiry = metadata.Expiry
                    };

                    var sessionId = await userSessions.CreateUserSession(session).ConfigureAwait(false);
                    if (sessionId.IsFailure)
                    {
                        return StatusCode(500);
                    }
                }
            }

            return Ok();
        }
    }
}
