# Webhooks - Updated to .NET 8 and Angular 17!
A standalone generic publish/subscribe notification system using HTTP webhooks and Hangfire as a fault tolerant event dispatcher.

## Architecture Overview
https://webhooks.hyprsoft.com/docs

### Sample Code
``` csharp
using Hyprsoft.Webhooks.Client;

// Webhooks REST client
var client = new WebhooksClient(options => options.ServerBaseUri = new Uri("https://webhooks.hyprsoft.com/"));

// Subscribe (a REST controller is needed to host the webhook callback endpoint)
var webhookUri = new Uri("https://office.hyprsoft.com/webhooks/v1/ping");
await client.SubscribeAsync<PingWebhookEvent>(webhookUri);

// Publish
await client.PublishAsync(new PingWebhookEvent());

// Unsubscribe
await client.UnsubscribeAsync<PingWebhookEvent>(webhookUri);
```

## Installation
###  Azure App Service
Simply deploy the Hyprsoft.Webhooks.Server.Web project to any Azure App service (tested on both Windows and Linux hosts).
#### App Service Configuration Changes
1. By default the webhooks server uses an in-memory data store.  Add an Azure SQL database connection string to use a persistent datastore.
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
       
            // Authentication is only required if your subscribing to any webhooks.
            builder.Services.AddWebhooksAuthentication(options => options.ApiKey = apiKey);
            builder.Services.AddWebhooksClient(options =>
            {
                options.ServerBaseUri = new Uri("https://webhooks.hyprsoft.com/");
                options.ApiKey = apiKey;
            });

            builder.Services.AddHostedService<WebhooksWorker>();

            var app = builder.Build();

            // Authentication is only required if you're subscribing to any webhooks.
            app.UseWebhooksAuthentication();

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

## Adding your own custom webhook events
You can dynamically add your own custom webhook events to the system by placing your custom .NET event assembly in the webhook server's bin folder and adding the name of the assembly to the server's application settings
configuration.
### Example
1. Add a new class library project to your solution.
2. Add a reference to the "Hyprsoft.Webhooks.Events" Nuget.
3. Add a new class to your new class library project inheriting from "Hyprsoft.Webhooks.Events.WebhookEvent".
4. Build your project in release and place the class library project's output assembly (and it's dependencies if applicable) in the webhook server's bin folder.  Your assembly must publically expose at least one type derived from "Hyprsoft.Webhooks.Events.WebhookEvent".


``` csharp
using Hyprsoft.Webhooks.Events;

namespace MyCompany.MyProduct1.WebhookEvents
{

    public class MyEvent : WebhookEvent
    {
        public string Message { get; set; }
    }
}
```
#### Configuration
``` json
"CustomEventAssemblyNames" : [ "MyCompany.MyProduct1.WebhookEvents.dll", "MyCompany.MyProduct2.WebhookEvents.dll", ... ] 
```
