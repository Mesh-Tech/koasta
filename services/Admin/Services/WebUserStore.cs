using CSharpFunctionalExtensions;
using Koasta.Shared.Crypto;
using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Koasta.Service.Admin.Services
{
    public class WebUserStore : IUserEmailStore<Employee>, IUserPasswordStore<Employee>, IPasswordHasher<Employee>, IUserRoleStore<Employee>, IUserSecurityStampStore<Employee>
    {
        private readonly EmployeeRepository _accountRepository;
        private readonly ICryptoHelper _cryptoHelper;
        private readonly EmployeeRoleRepository roles;

        public WebUserStore(EmployeeRepository accountRepository, EmployeeRoleRepository roles, ICryptoHelper cryptoHelper)
        {
            _accountRepository = accountRepository;
            _cryptoHelper = cryptoHelper;
            this.roles = roles;
        }

        public async Task<IdentityResult> CreateAsync(Employee user, CancellationToken _)
        {
            user.PasswordHash ??= "";
            return (await _accountRepository.CreateEmployee(user).ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Employee was created")
                .OnSuccess(e => {
                    user.EmployeeId = e.Value;
                    return e.Value;
                })
                .OnBoth(e => e.IsFailure
                    ? IdentityResult.Failed(new IdentityError { Code = "EXC", Description = "Failed to save employee" })
                    : IdentityResult.Success);
        }

        public async Task<IdentityResult> DeleteAsync(Employee user, CancellationToken cancellationToken)
        {
            return (await _accountRepository.DropEmployee(user.EmployeeId).ConfigureAwait(false))
                .OnBoth(e => e.IsFailure
                    ? IdentityResult.Failed(new IdentityError { Code = "EXC", Description = "Failed to delete employee" })
                    : IdentityResult.Success);
        }

        public async Task<Employee> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return (await _accountRepository.FetchEmployee(int.Parse(userId)).ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Employee is available")
                .OnBoth(e => e.IsFailure
                    ? null
                    : e.Value.Value);
        }

        public async Task<Employee> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return (await _accountRepository.FetchEmployeeByUsername(normalizedUserName.ToLower()).ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Employee is available")
                .OnBoth(e => e.IsFailure
                    ? null
                    : e.Value.Value);
        }

        public Task<string> GetNormalizedUserNameAsync(Employee user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Username.ToLower());
        }

        public Task<string> GetUserIdAsync(Employee user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmployeeId.ToString());
        }

        public Task<string> GetUserNameAsync(Employee user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Username.ToLower());
        }

        public Task SetNormalizedUserNameAsync(Employee user, string normalizedName, CancellationToken cancellationToken)
        {
            user.Username = normalizedName.ToLower();
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(Employee user, string userName, CancellationToken cancellationToken)
        {
            user.Username = userName.ToLower();
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(Employee user, CancellationToken cancellationToken)
        {
            return (await _accountRepository.ReplaceEmployee(user.EmployeeId, user).ConfigureAwait(false))
                .OnBoth(e => e.IsFailure
                    ? IdentityResult.Failed(new IdentityError { Code = "EXC", Description = "Failed to replace employee" })
                    : IdentityResult.Success);
        }

        public void Dispose()
        {
        }

        public Task SetEmailAsync(Employee user, string email, CancellationToken cancellationToken)
        {
            user.Username = email;
            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(Employee user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Username);
        }

        public Task<bool> GetEmailConfirmedAsync(Employee user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Confirmed);
        }

        public Task SetEmailConfirmedAsync(Employee user, bool confirmed, CancellationToken cancellationToken)
        {
            user.Confirmed = true;
            return Task.CompletedTask;
        }

        public async Task<Employee> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return (await _accountRepository.FetchEmployeeByUsername(normalizedEmail.ToLower()).ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Employee is available")
                .OnBoth(e => e.IsFailure
                    ? null
                    : e.Value.Value);
        }

        public Task<string> GetNormalizedEmailAsync(Employee user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Username.ToLower());
        }

        public Task SetNormalizedEmailAsync(Employee user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.Username = normalizedEmail.ToLower();
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(Employee user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(Employee user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(Employee user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }

        public string HashPassword(Employee user, string password)
        {
            return _cryptoHelper.Generate(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(Employee user, string hashedPassword, string providedPassword)
        {
            return _cryptoHelper.IsValid(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }

        public async Task AddToRoleAsync(Employee user, string roleName, CancellationToken cancellationToken)
        {
            var role = (await roles.FetchEmployeeRoleByName(roleName).ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Role is available")
                .OnSuccess(e => e.Value);

            if (role.IsFailure)
            {
                throw new InvalidOperationException("Failed to fetch role");
            }

            user.RoleId = role.Value.RoleId;
        }

        public async Task RemoveFromRoleAsync(Employee user, string roleName, CancellationToken cancellationToken)
        {
            var role = (await roles.FetchEmployeeRoleByName("None").ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Role is available")
                .OnSuccess(e => e.Value);

            if (role.IsFailure)
            {
                throw new InvalidOperationException("Failed to fetch None role");
            }

            user.RoleId = role.Value.RoleId;
        }

        public async Task<IList<string>> GetRolesAsync(Employee user, CancellationToken cancellationToken)
        {
            var results = await roles.FetchEmployeeRole(user.RoleId)
                .Ensure(e => e.HasValue, "Role is available")
                .OnSuccess(e => e.Value)
                .ConfigureAwait(false);

            if (results.IsFailure) {
                return new List<string>();
            }

            return new List<string> { results.Value.RoleName };
        }

        public async Task<bool> IsInRoleAsync(Employee user, string roleName, CancellationToken cancellationToken)
        {
            var role = (await roles.FetchEmployeeRoleByName("None").ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Role is available")
                .OnSuccess(e => e.Value);

            if (role.IsFailure)
            {
                throw new InvalidOperationException("Failed to fetch None role");
            }

            return user.RoleId == role.Value.RoleId;
        }

        public async Task<IList<Employee>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var role = (await roles.FetchEmployeeRoleByName("None").ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Role is available")
                .OnSuccess(e => e.Value);

            if (role.IsFailure)
            {
                throw new InvalidOperationException("Failed to fetch None role");
            }

            return (await _accountRepository.FetchEmployeeByRoleId(role.Value.RoleId).ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Employees are available")
                .OnSuccess(e => e.Value)
                .OnBoth(e => e.IsSuccess ? e.Value : new List<Employee>());
        }

        public Task SetSecurityStampAsync(Employee user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task<string> GetSecurityStampAsync(Employee user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }
    }
}
