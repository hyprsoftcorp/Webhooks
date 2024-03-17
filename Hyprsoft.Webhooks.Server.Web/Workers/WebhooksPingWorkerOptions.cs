namespace Hyprsoft.Webhooks.Server.Web
{
    public sealed class WebhooksPingWorkerOptions
    {
        public TimeSpan PublishPingEventInterval { get; set; } = TimeSpan.FromMinutes(1);
    }
}
