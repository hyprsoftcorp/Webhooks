using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Hyprsoft.Webhooks.Server
{
    internal class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context) => true;
    }
}
