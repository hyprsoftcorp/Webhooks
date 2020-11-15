using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Hyprsoft.Webhooks.Core.Hangfire
{
    internal class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context) => true;
    }
}
