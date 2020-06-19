using System;

namespace Hyprsoft.Webhooks.Core.Rest
{
    public class WebhooksHttpClientOptions
    {
        public Uri ServerBaseUri { get; set; } = new Uri("http://localhost:5000/");

        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public string PayloadSigningSecret { get; set; } = WebhooksGlobalConfiguration.DefaultPayloadSigningSecret;
    }
}
