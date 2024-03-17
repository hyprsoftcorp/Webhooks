using Hyprsoft.Webhooks.Core;

namespace Hyprsoft.Webhooks.Client
{
    public sealed class WebhooksHttpClientOptions
    {
        public static readonly Uri DefaultServerBaseUri = new("http://localhost:5000/");

        public Uri ServerBaseUri { get; set; } = DefaultServerBaseUri;

        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public string ApiKey { get; set; } = WebhooksGlobalConfiguration.DefaultApiKey;
    }
}
