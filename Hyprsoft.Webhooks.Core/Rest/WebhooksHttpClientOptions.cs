using System;

namespace Hyprsoft.Webhooks.Core.Rest
{
    public class WebhooksHttpClientOptions
    {
        public static readonly Uri DefaultServerBaseUri = new Uri("http://localhost:5000/");

        public Uri ServerBaseUri { get; set; } = DefaultServerBaseUri;

        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public string ApiKey { get; set; } = WebhooksGlobalConfiguration.DefaultApiKey;
    }
}
