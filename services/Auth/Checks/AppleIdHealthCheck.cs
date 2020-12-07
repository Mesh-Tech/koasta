using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Koasta.Service.Auth.Utils;

namespace Koasta.Service.Auth.Checks
{
    public class AppleIdHealthCheck : IHealthCheck
    {
        private readonly AppleJWTKeyRefresher keyRefresher;

        public AppleIdHealthCheck(AppleJWTKeyRefresher keyRefresher)
        {
            this.keyRefresher = keyRefresher;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (keyRefresher.Ready)
            {
                return Task.FromResult(HealthCheckResult.Healthy());
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("Still fetching Apple ID signing keys"));
        }
    }
}
