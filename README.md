# Webhooks - Updated to .NET 5 and Angular 11!
A standalone generic publish/subscribe notification system using HTTP webhooks and Hangfire as a fault tolerant event dispatcher.

## Architecture Overview
https://webhooks.hyprsoft.com/docs

### Sample Code
``` csharp
// Webhooks REST client
var client = new WebhooksClient(new WebhooksHttpClientOptions { ServerBaseUri = new Uri("https://webhooks.hyprsoft.com/") });

// Subscribe
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
1. By default the webhooks server uses an in memory data store.  Add an app service configuration setting "UseInMemoryDatastore = false" to use SQL Server instead.
2. Ensure an Azure SQL database named "WebhooksDb" exists and add/update the "Webhooksdb" connection string in your app configuration accordingly.
3. Change your playload signing secret!  Add an app service configuration setting "PayloadSigningSecret = [MyFancyNewSecret]".
### Add to an existing ASP.NET Core Website
You'll need a reference to the Hyprsoft.Webhooks.AspNetCore assembly.
#### Startup (Web API)
``` csharp
using Hyprsoft.Webhooks.AspNetCore;
using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Using Newtonsoft.Json here is currently required (a fix is coming).
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.TypeNameHandling = WebhooksGlobalConfiguration.JsonSerializerSettings.TypeNameHandling);
            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication1", Version = "v1" }));
            
            services.AddWebhooksClient(options => options.ServerBaseUri = new Uri("https://webhooks.hyprsoft.com/"));
            services.AddWebhooksAuthorization();
            services.AddHostedService<StartupWorker>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseWebhooksAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication1 v1"));
        }
    }
}

```
#### Startup Worker
``` csharp
using Hyprsoft.Webhooks.Core.Events;
using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class StartupWorker : BackgroundService
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
            // You may not want to unsubscibe if the webhook events are "mission critical".
            await _webhooksClient.UnsubscribeAsync<PingWebhookEvent>(_webhookBaseUri);
            await base.StopAsync(stoppingToken);
        }
    }
}
```
#### Webhooks Controller
``` csharp
using Hyprsoft.Webhooks.AspNetCore;
using Hyprsoft.Webhooks.Core.Events;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebApplication1.Controllers
{
    // Unless you're using API versioning (and you should be), this is required since the WebhooksControllerBase is decorated with an API versioned route.
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

## TODOs
1. Add the ability to use System.Text.Json side-by-side with Newtonsoft.Json.  https://blogs.taiga.nl/martijn/2020/05/28/system-text-json-and-newtonsoft-json-side-by-side-in-asp-net-core/ 
2. Figure out a way to balance performance and usability/effectivness when it comes to logging errors in the audit table.  Currently the audit table's error column is nvarchar(1024) and we only audit the exception's message property.  We'd like to audit the exception's call stack which can certainly exceed 1024 characters.  If we alter the column to be nvarchar(max) we will eventually run into performance issues generating the failed webhooks data in the health summary which groups by event name, webhook URI, and error message.
3. Fluent API implementation?
4. gRPC implementation?
