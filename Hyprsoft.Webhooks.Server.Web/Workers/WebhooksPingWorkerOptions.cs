namespace Hyprsoft.Webhooks.Server.Web
{
    public class WebhooksPingWorkerOptions
    {
        public TimeSpan PublishPingEventInterval { get; set; } = TimeSpan.FromMinutes(1);
    }
}
