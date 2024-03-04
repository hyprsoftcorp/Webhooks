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
        public static readonly Uri DefaultServerBaseUri = new("http://localhost:5000/");

        public static readonly Uri DefaultWebhooksBaseUri = new($"http://localhost:5001/");

        public Uri ServerBaseUri { get; set; } = DefaultServerBaseUri;

        public Uri WebhooksBaseUri { get; set; } = DefaultWebhooksBaseUri;

        public WebhooksWorkerRole Role { get; set; } = WebhooksWorkerRole.PubSub;

        public bool AutoUnsubscribe { get; set; } = true;
    }
}
