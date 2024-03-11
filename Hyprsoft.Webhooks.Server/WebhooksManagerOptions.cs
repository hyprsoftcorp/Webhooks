using Hyprsoft.Webhooks.Client;

namespace Hyprsoft.Webhooks.Server
{
    public class WebhooksManagerOptions
    {
        public string? DatabaseConnectionString { get; set; }

        public WebhooksHttpClientOptions HttpClientOptions { get; set; } = new();
    }
}
