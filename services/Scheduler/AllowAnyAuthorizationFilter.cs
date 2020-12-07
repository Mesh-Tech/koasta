using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Koasta.Service.Scheduler
{
    public class AllowAnyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            return true;
        }
    }
}
