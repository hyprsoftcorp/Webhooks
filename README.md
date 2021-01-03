# Webhooks - Updated to .NET 5 and Angular 11!
A standalone generic publish/subscribe notification system using HTTP webhooks and Hangfire as a fault tolerant event dispatcher.

## Documentation
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

// Unsubscibe
await client.UnsubscribeAsync<PingWebhookEvent>(webhookUri);
```

## Installation
###  Azure App Service
Simply deploy the Hyprsoft.Webhooks.Server.Web project to any Azure App service (only tested on Windows hosts).
#### App Service Configuration Changes
1. By default the webhooks server uses an in memory data store.  Add an app service configuration setting "UseInMemoryDatastore = false" to change that behavior.
2. Ensure an Azure SQL database named "WebhooksDb" exists and add/update the "Webhooksdb" connection string accordingly.

### Windows Service
``` 
sc create HyprsoftWebhooksServer binpath= "C:\Source\Hyprsoft.Webhooks.Solution\Hyprsoft.Webhooks.Server.Web\bin\Release\net5.0\win-x64\publish\Hyprsoft.Webhooks.Server.Web.exe  --service 1 --urls http://*:80" DisplayName= "Hyprsoft Webhooks Server" start= auto
sc description HyprsoftWebhooksServer "A standalone generic publish/subscribe notification system using HTTP webhooks and Hangfire as a fault tolerant event dispatcher." 
net start HyprsoftWebhooksServer

sc query HyprsoftWebhooksServer 
net stop HyprsoftWebhooksServer 
sc delete HyprsoftWebhooksServer 
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
1. Figure out a way to balance performance and usability/effectivness when it comes to logging errors in the audit table.  Currently the audit table's error column is nvarchar(1024) and we only audit the exception's message property.  We'd like to audit the exception's call stack which can certainly exceed 1024 characters.  If we alter the column to be nvarchar(max) we will eventually run into performance issues generating the failed webhooks data in the health summary which groups by event name, webhook URI, and error message.
2. Fluent API implementation?
3. gRPC implementation?
