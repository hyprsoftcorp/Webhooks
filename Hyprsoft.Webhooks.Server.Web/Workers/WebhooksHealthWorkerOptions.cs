using System;

namespace Hyprsoft.Webhooks.Server.Web
{
    public class WebhooksHealthWorkerOptions
    {
        public TimeSpan PublishHealthInterval { get; set; } = TimeSpan.FromHours(1);
    }
}
