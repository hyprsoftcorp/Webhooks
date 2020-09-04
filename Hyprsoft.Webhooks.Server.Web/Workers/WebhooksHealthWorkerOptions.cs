using System;

namespace Hyprsoft.Webhooks.Server.Web
{
    public class WebhooksHealthWorkerOptions
    {
        public TimeSpan PublishHealthEventInterval { get; set; } = TimeSpan.FromHours(1);
    }
}
