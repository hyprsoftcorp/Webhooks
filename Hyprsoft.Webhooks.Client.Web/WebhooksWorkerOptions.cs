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
        public static readonly Uri DefaultServerBaseUri = new Uri("http://localhost:5000/");

        public static readonly Uri DefaultWebhooksBaseUri = new Uri($"http://localhost:5001/");

        public Uri ServerBaseUri { get; set; } = DefaultServerBaseUri;

        public Uri WebhooksBaseUri { get; set; } = DefaultWebhooksBaseUri;

        public WebhooksWorkerRole Role { get; set; } = WebhooksWorkerRole.PubSub;

        public TimeSpan PublishInterval { get; set; } = TimeSpan.FromSeconds(10);

        public int MaxEventsToPublishPerInterval { get; set; } = 10;

        public bool AutoUnsubscribe { get; set; } = true;
    }
}
