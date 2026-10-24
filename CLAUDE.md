# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

A standalone publish/subscribe notification system: clients subscribe HTTP callback URLs to event types, publishers post events to the server, and the server fans each event out to subscribers through Hangfire background jobs (retries with backoff). .NET 10 backend, Angular 21 dashboard SPA. Public docs: https://webhooks.hyprsoft.com/docs

## Commands

```bash
# Build everything (Server.Web references the Angular .esproj, so this also builds the SPA)
dotnet build Hyprsoft.Webhooks.Solution.sln

# Run the server (http://localhost:5000). In Development, SpaProxy launches `npm start` for the SPA on :4200.
dotnet run --project Hyprsoft.Webhooks.Server.Web

# Run the sample client (http://localhost:5001). Settings come from config/command line:
# ServerBaseUri, WebhooksBaseUri, Role (PubSub | Pub | Sub), AutoUnsubscribe
dotnet run --project Hyprsoft.Webhooks.Client.Web -- --ServerBaseUri "http://localhost:5000/" --Role PubSub

# Tests (MSTest)
dotnet test Hyprsoft.Webhooks.Tests
dotnet test Hyprsoft.Webhooks.Tests --filter "FullyQualifiedName~Subscribe_Unsubscribe_Success"

# Angular SPA (run from Hyprsoft.Webhooks.Server.Spa)
npm start        # ng serve, dev environment points at http://localhost:5000
npm run build    # output: dist/hyprsoft.webhooks.server.spa/browser
```

Note: every test in `Hyprsoft.Webhooks.Tests/WebhooksClientTests.cs` is currently commented out (they use the old `UseStartup` hosting model), so `dotnet test` runs nothing until they are rewritten.

## Project layout and dependencies

- **Events**: `WebhookEvent` abstract base (`WebhookId`, `CreatedUtc`, `Originator`) and `PingWebhookEvent`. The only package custom-event authors reference.
- **Core**: entities (`Subscription`, `Audit`), `WebhookException`, `BuildInfo`, and `WebhooksGlobalConfiguration` (default API key, DB name, shared JSON settings).
- **Client** (depends on Core and Events): `IWebhooksClient`/`WebhooksClient` (typed `HttpClient`), the `X-API-KEY` `WebhooksMessageHandler`, API-key authentication setup, and `WebhooksControllerBase` for subscribers hosting callback endpoints. DI entry points are `AddWebhooksClient`, `AddWebhooksAuthentication` and `UseWebhooksAuthentication`.
- **Server** (depends on Client): `IWebhooksManager`, `WebhooksManager`/`HangfireWebhooksManager`, EF Core `WebhooksDbContext`/`WebhooksRepository`, Hangfire wiring. DI entry points are `AddWebhooksServer` and `UseWebhooksServer`.
- **Server.Web**: ASP.NET Core host. Controllers, Swagger, health checks, `WebhooksPingWorker`, and the SPA served from static files with an `index.html` fallback.
- **Server.Spa**: Angular dashboard (home/docs/health/subscriptions) that calls the Server.Web `api/v1/*` endpoints.
- **Client.Web**: sample/test client. `WebhooksWorker` subscribes and/or publishes pings depending on `Role`, and `V1/Controllers/WebhooksController` receives the callbacks.

Core, Client, Events and Server are published as NuGet packages (`deploy-nugets.cmd`). Versions come from git tags via **MinVer**, not from the csproj files.

NuGet **Central Package Management** is enabled: every package version lives in the root `Directory.Packages.props`, and `<PackageReference>` items in csproj files must not have a `Version` (NuGet fails with NU1008 if they do). To add or upgrade a package, edit `Directory.Packages.props`. The `Hyprsoft.Webhooks.AspNetCore` folder holds only stale build output and is not part of the solution.

## Architecture: how an event flows

1. **Event identity is the CLR full type name.** `SubscribeAsync<TEvent>` sends `typeof(TEvent).FullName`, and the server matches subscriptions on `@event.GetType().FullName`.
2. **Polymorphic JSON with Newtonsoft `TypeNameHandling.All`** (`WebhooksGlobalConfiguration.JsonSerializerSettings`). `WebhookContent` serializes with `$type`, and the server's `Publish(WebhookEvent @event)` action relies on it to deserialize the concrete type. As a result the server must be able to load every event type. Custom event assemblies are dropped into the server's bin folder and listed in the `CustomEventAssemblyNames` config; `AddWebhooksServer` loads them at startup. The whole stack uses Newtonsoft.Json (`AddNewtonsoftJson`), not System.Text.Json.
3. **Subscription filters**: the client's `Expression<Func<TEvent,bool>>` is converted to a Serialize.Linq `ExpressionNode`. The server stores it as JSON in `Subscription.FilterExpression`, then deserializes, compiles and invokes it against each published event in `WebhooksManager.PublishAsync`.
4. **Dispatch**: `PublishAsync` writes a Publish audit, then `HangfireWebhooksManager.OnPublishAsync` enqueues `IWebhooksManager.DispatchAsync` per matching subscription. Hangfire resolves `IWebhooksManager` from the app's container through `HangfireActivator`. `DispatchAsync` POSTs the event to the subscriber, records a Dispatch audit (with any error), and rethrows on failure so Hangfire retries.
5. **Auth is a single shared API key** in the `X-API-KEY` header. The server validates incoming requests with it and also sends it on outbound dispatches, so subscriber endpoints (`WebhooksControllerBase` has `[Authorize]` with the ApiKey scheme) must be configured with the same key. Configure it with the `ApiKey` setting; it defaults to `WebhooksGlobalConfiguration.DefaultApiKey`.
6. **Storage switch**: if `ConnectionStrings:WebhooksDb` is empty, both EF Core and Hangfire use in-memory stores; otherwise both use SQL Server. `WebhooksDb.sql` is the SQL schema script. `AddWebhooksServer` instantiates a `WebhooksDbContext` before configuring Hangfire, a deliberate workaround (see the TODO there) so the database exists before Hangfire initializes. Keep that ordering.

### Routes (lowercase URLs, API versioning defaults to v1)
- `webhooks/v1/subscribe` (PUT), `webhooks/v1/unsubscribe` (DELETE), `webhooks/v1/publish` (POST): API-key protected.
- `api/v1/buildinfo`, `api/v1/health?period=`, `api/v1/subscriptions`: used by the SPA.
- `/hangfire` (dashboard; `HangfireAuthorizationFilter` currently allows everyone), `/swagger`, `/api/health`.

Swagger reads `Hyprsoft.Webhooks.Server.Web.xml`, which is generated from the controllers' XML doc comments. The csproj sets `DocumentationFile` to an absolute `D:\Source\...` path, so builds from a different clone location write the file somewhere else.

### Environments and config
- The `UnitTest` environment skips the hosted workers (`WebhooksPingWorker`, `WebhooksWorker`) in both web hosts.
- CORS origins come from the `corsOrigins` array (`http://localhost:4200` in Development).
- Passing `--service` as the first argument (with at least one more argument) runs Server.Web as a Windows Service.
- Application Insights is enabled only when a connection string is configured.

## Conventions

C# classes are organized with `#region Fields / Constructors / Properties / Methods`. Each library exposes `Add*`/`Use*` extension methods with three overloads: no options, an `Action<TOptions>`, and a `TOptions` instance. Follow that pattern for new options.
