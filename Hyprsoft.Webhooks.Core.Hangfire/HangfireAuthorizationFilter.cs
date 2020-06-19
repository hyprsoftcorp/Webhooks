using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Hyprsoft.Webhooks.Core.Hangfire
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context) => true;
    }
}
