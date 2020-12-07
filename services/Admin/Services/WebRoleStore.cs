using CSharpFunctionalExtensions;
using Koasta.Shared.Database;
using Koasta.Shared.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace Koasta.Service.Admin.Services
{
    public class WebRoleStore : IRoleStore<EmployeeRole>
    {
        private readonly EmployeeRoleRepository roles;

        public WebRoleStore(EmployeeRoleRepository roles)
        {
            this.roles = roles;
        }

        public Task<IdentityResult> CreateAsync(EmployeeRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(EmployeeRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public async Task<EmployeeRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return (await roles.FetchEmployeeRole(int.Parse(roleId)).ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Role is available")
                .OnBoth(e => e.IsFailure
                    ? null
                    : e.Value.Value);
        }

        public async Task<EmployeeRole> FindByNameAsync(string roleName, CancellationToken cancellationToken)
        {
            return (await roles.FetchEmployeeRoleByName(roleName).ConfigureAwait(false))
                .Ensure(e => e.HasValue, "Role is available")
                .OnBoth(e => e.IsFailure
                    ? null
                    : e.Value.Value);
        }

        public Task<string> GetNormalizedRoleNameAsync(EmployeeRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.RoleName);
        }

        public Task<string> GetRoleIdAsync(EmployeeRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.RoleId.ToString());
        }

        public Task<string> GetRoleNameAsync(EmployeeRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.RoleName);
        }

        public Task SetNormalizedRoleNameAsync(EmployeeRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.RoleName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(EmployeeRole role, string roleName, CancellationToken cancellationToken)
        {
            role.RoleName = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(EmployeeRole role, CancellationToken cancellationToken)
        {
            return (await roles.ReplaceEmployeeRole(role.RoleId, role).ConfigureAwait(false))
                .OnBoth(e => e.IsFailure
                    ? IdentityResult.Failed(new IdentityError { Code = "EXC", Description = "Failed to replace role" })
                    : IdentityResult.Success);
        }

        public void Dispose()
        {
        }
    }
}
