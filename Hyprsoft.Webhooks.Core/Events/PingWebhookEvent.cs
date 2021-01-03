namespace Hyprsoft.Webhooks.Core.Events
{
    public class PingWebhookEvent : WebhookEvent
    {
        public bool IsException { get; set; }
    }
}
