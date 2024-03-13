namespace Hyprsoft.Webhooks.Events
{
    public class PingWebhookEvent : WebhookEvent
    {
        public bool IsException { get; set; }
    }
}
