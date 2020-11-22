# Webhooks - Updated to .NET 5!
A standalone generic publish/subscribe notification system using HTTP webhooks and Hangfire as a fault tolerant event dispatcher.

## Documentation
https://webhooks.hyprsoft.com/docs

### Sample Code
``` csharp
// Webhooks REST client
var client = new WebhooksClient(new WebhooksHttpClientOptions { ServerBaseUri = new Uri("http://webhooks.hyprsoft.com/") });

// Subscribe
var webhookUri = new Uri("http://webhooks.hyprsoft.com/webhooks/v1/samplecreated");
await client.SubscribeAsync<SampleCreatedWebhookEvent>(webhookUri, x => x.SampleType == 2);

// Publish
await client.PublishAsync(new SampleCreatedWebhookEvent { SampleId = 1, SampleType = 2, UserId = 3, ReferenceId = 4 });

// Unsubscibe
await client.UnsubscribeAsync<SampleCreatedWebhookEvent>(webhookUri);
```

## Installation
###  Azure App Service
Simply deploy the Hyprsoft.Webhooks.Server.Web project to any Azure App service (only tested on Windows hosts).
#### App Service Configuration Changes
1. By default the webhooks server uses an in memory data store.  Add an app service configuration setting "UseInMemoryDatastore = false" to change that behavior.
2. Ensure an Azure SQL database named "WebhooksDb" exists and add/update the "Webhooksdb" connection string accordingly.

### Windows Service
``` 
sc create HyprsoftWebhooksServer binpath= "C:\Source\Hyprsoft.Webhooks.Solution\Hyprsoft.Webhooks.Server.Web\bin\Release\netcoreapp5.0\win-x64\publish\Hyprsoft.Webhooks.Server.Web.exe  --service 1 --urls http://*:80" DisplayName= "Hyprsoft Webhooks Server" start= auto
sc description HyprsoftWebhooksServer "A standalone generic publish/subscribe notification system using HTTP webhooks and Hangfire as a fault tolerant event dispatcher." 
net start HyprsoftWebhooksServer

sc query HyprsoftWebhooksServer 
net stop HyprsoftWebhooksServer 
sc delete HyprsoftWebhooksServer 
```

## Testing
### Example Client Web Command Line
```
Hyprsoft.Webhooks.Client.Web.exe ServerBaseUri="https://webhooks.hyprsoft.com/" WebhooksBaseUri="http://office.hyprsoft.com/" Role=PubSub AutoUnsubscribe=true
```

## Future Plans
1. Fluent API implementation.
2. gRPC implementation.