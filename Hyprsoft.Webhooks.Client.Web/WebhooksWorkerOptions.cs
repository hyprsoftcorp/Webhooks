using System;

namespace Hyprsoft.Webhooks.Client.Web
{
    public enum WebhooksWorkerRole
    {
        PubSub,
        Pub,
        Sub
    }

    public class WebhooksWorkerOptions
    {
        public Uri ServerBaseUri { get; set; } = new Uri("http://localhost:5000/");

        public Uri WebhooksBaseUri { get; set; } = new Uri($"http://localhost:5001/");

        public WebhooksWorkerRole Role { get; set; } = WebhooksWorkerRole.PubSub;

        public TimeSpan PublishInterval { get; set; } = TimeSpan.FromSeconds(10);

        public int MaxEventsToPublishPerInterval { get; set; } = 10;

        public bool AutoUnsubscribe { get; set; } = true;
    }
}
