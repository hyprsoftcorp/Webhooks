using Hyprsoft.Webhooks.Core.Management;

namespace Hyprsoft.Webhooks.Core.Events
{
    public class WebhooksHealthEvent : WebhookEvent
    {
        public WebhooksHealthSummary Summary { get; set; }
    }
}
