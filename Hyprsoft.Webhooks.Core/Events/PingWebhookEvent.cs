namespace Hyprsoft.Webhooks.Core
{
    public class PingWebhookEvent : WebhookEvent
    {
        public bool IsException { get; set; }
    }
}
