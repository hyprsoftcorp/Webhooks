namespace Hyprsoft.Webhooks.Events
{
    public sealed class PingEvent : WebhookEvent
    {
        public bool IsException { get; set; }
    }
}
