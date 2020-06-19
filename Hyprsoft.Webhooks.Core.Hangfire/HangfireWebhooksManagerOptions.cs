using Hyprsoft.Webhooks.Core.Rest;

namespace Hyprsoft.Webhooks.Core.Hangfire
{
    public class HangfireWebhooksManagerOptions
    {
        public string DatabaseConnectionString { get; set; } = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=WebhooksDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True";

        public WebhooksHttpClientOptions HttpClientOptions { get; set; } = new WebhooksHttpClientOptions();
    }
}
