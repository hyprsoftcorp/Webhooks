# Webhooks - Updated to .NET 8 and Angular 17!
A standalone generic publish/subscribe notification system using HTTP webhooks and Hangfire as a fault tolerant event dispatcher.

## Architecture Overview
https://webhooks.hyprsoft.com/docs

### Sample Code
``` csharp
using Hyprsoft.Webhooks.Client;

// Webhooks REST client
var client = new WebhooksClient(options => options.ServerBaseUri = new Uri("https://webhooks.hyprsoft.com/"));

// Subscribe (a REST controller is needed to host the webhook callback endpoints)
var webhookUri = new Uri("https://office.hyprsoft.com/webhooks/v1/ping");
await client.SubscribeAsync<PingWebhookEvent>(webhookUri);

// Publish
await client.PublishAsync(new PingWebhookEvent());

// Unsubscribe
await client.UnsubscribeAsync<PingWebhookEvent>(webhookUri);
```

## Installation
###  Azure App Service
Simply deploy the Hyprsoft.Webhooks.Server.Web project to any Azure App service (only tested on Windows hosts).
#### App Service Configuration Changes
1. By default the webhooks server uses an in memory data store.  Add an Azure SQL database connection string to use a persistent datastore.
2. Change your api key!  Add an app service configuration setting "ApiKey = [MyFancyNewApiKey]".
### Sample Code
You'll need a reference to the Hyprsoft.Webhooks.Client Nuget.
#### Webhooks Client Web
``` csharp
using Hyprsoft.Webhooks.Core;

namespace Hyprsoft.Webhooks.Client.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var apiKey = "my-secret-api-key";

            builder.Services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.TypeNameHandling = WebhooksGlobalConfiguration.JsonSerializerSettings.TypeNameHandling);
            builder.Services.AddHostedService<WebhooksWorker>();

            // Authentication is only required if your subscribing to any webhooks.
            builder.Services.AddWebhooksAuthentication(options => options.ApiKey = apiKey);
            builder.Services.AddWebhooksClient(options =>
            {
                options.ServerBaseUri = new Uri("https://webhooks.hyprsoft.com");
                options.ApiKey = apiKey;
            });

            var app = builder.Build();

            // Authentication is only required if your subscribing to any webhooks.
            app.UseWebhooksAuthentication();
            app.MapControllers();

            app.Run();
        }
    }
}
```
#### Webhooks Worker
``` csharp
using Hyprsoft.Webhooks.Core;

namespace Hyprsoft.Webhooks.Client.Web
{
    public class WebhooksWorker : BackgroundService
    {
        private readonly IWebhooksClient _webhooksClient;
        private readonly Uri _webhookBaseUri = new Uri("http://office.hyprsoft.com/webhooks/ping");

        public StartupWorker(IWebhooksClient webhooksClient) => _webhooksClient = webhooksClient;

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            await _webhooksClient.SubscribeAsync<PingWebhookEvent>(_webhookBaseUri);
            await base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) => await Task.Delay(0, stoppingToken);

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _webhooksClient.UnsubscribeAsync<PingWebhookEvent>(_webhookBaseUri);
            await base.StopAsync(stoppingToken);
        }
    }
}
```
#### Webhooks Controller
``` csharp
using Hyprsoft.Webhooks.Core;

namespace Hyprsoft.Webhooks.Client.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class WebhooksController : WebhooksControllerBase
    {
        [HttpPost]
        public IActionResult Ping(PingWebhookEvent @event)
        {
            try
            {
                // Do something amazing with this webhook event!
                return WebhookOk();
            }
            catch (Exception ex)
            {
                return WebhookException(ex);
            }
        }
    }
}
```

### As a Windows Service
```
sc create HyprsoftWebhooksServer binpath= "C:\Source\Hyprsoft.Webhooks.Solution\Hyprsoft.Webhooks.Server.Web\bin\Release\net5.0\win-x64\publish\Hyprsoft.Webhooks.Server.Web.exe  --service 1 --urls http://*:80" DisplayName= "Hyprsoft Webhooks Server" start= auto
sc description HyprsoftWebhooksServer "A standalone generic publish/subscribe notification system using HTTP webhooks and Hangfire as a fault tolerant event dispatcher." 
net start HyprsoftWebhooksServer
```

## Testing
### Example Client Web Command Line
#### Windows
```
Hyprsoft.Webhooks.Client.Web.exe ServerBaseUri="https://webhooks.hyprsoft.com/" WebhooksBaseUri="http://office.hyprsoft.com/" Role=PubSub
```
#### Linux
```
./Hyprsoft.Webhooks.Client.Web --ServerBaseUri "https://webhooks.hyprsoft.com/" --WebhooksBaseUri "http://office.hyprsoft.com/" --Role PubSub
```

## Adding your own webhook events
Coming soon.