# Hyprsoft Webhooks
This is an implementation of a stand-alone generic publish/subscribe notification system using webhooks and Hangfire.

Documentation: [https://hyprsoftwebhooks.azurewebsites.net/](https://hyprsoftwebhooks.azurewebsites.net/)

## Install as Windows Service
```
sc create HyprsoftWebhooksServer binpath= "D:\Source\Hyprsoft.Webhooks.Solution\Hyprsoft.Webhooks.Server.Web\bin\Release\netcoreapp3.1\win-x64\publish\Hyprsoft.Webhooks.Server.Web.exe  --service 1" DisplayName= "Hyprsoft Webhooks Server" start= auto
sc description HyprsoftWebhooksServer "Hyprsoft Webhooks Server" 
net start HyprsoftWebhooksServer

sc query HyprsoftWebhooksServer 
net stop HyprsoftWebhooksServer 
sc delete HyprsoftWebhooksServer 
```

## Sample Client Web Command Line
```
Hyprsoft.Webhooks.Client.Web.exe ServerBaseUri="https://hyprsoftwebhooks.azurewebsites.net/" WebhookBaseUri="http://office.hyprsoft.com/" Role=PubSub AutoUnsubscribe=true
```