using Hyprsoft.Webhooks.Client;

namespace Hyprsoft.Webhooks.Server
{
    public sealed class WebhooksManagerOptions
    {
        public string? DatabaseConnectionString { get; set; }

        public WebhooksHttpClientOptions HttpClientOptions { get; set; } = new();

        public IEnumerable<string> CustomEventAssemblyNames { get; set; } = [];
    }
}
