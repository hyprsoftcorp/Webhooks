namespace Hyprsoft.Webhooks.Events
{
    public class PingEvent : WebhookEvent
    {
        public bool IsException { get; set; }
    }
}
