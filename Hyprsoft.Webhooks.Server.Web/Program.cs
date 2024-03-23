using Hyprsoft.Webhooks.Client;
using Hyprsoft.Webhooks.Core;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Hyprsoft.Webhooks.Server.Web
{
    public sealed class Program
    {
        public static DateTime ServerStartDateUtc { get; private set; }

        public static void Main(string[] args)
        {
            ServerStartDateUtc = DateTime.UtcNow;

            var builder = WebApplication.CreateBuilder(args);
            var apiKey = builder.Configuration.GetValue(nameof(WebhooksAuthenticationOptions.ApiKey), WebhooksGlobalConfiguration.DefaultApiKey)!;

            if (args.Length > 1 && args[0].Contains("service"))
                builder.Services.AddWindowsService();

            builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            builder.Services.AddApiVersioning(options => options.AssumeDefaultVersionWhenUnspecified = true);
            builder.Services.AddWebhooksAuthentication(options => options.ApiKey = apiKey);
            builder.Services.AddWebhooksServer(builder.Configuration, options =>
            {
                options.DatabaseConnectionString = builder.Configuration.GetConnectionString(WebhooksGlobalConfiguration.WebhooksDbName);
                options.HttpClientOptions.ApiKey = apiKey;
                options.CustomEventAssemblyNames = builder.Configuration.GetSection(nameof(WebhooksManagerOptions.CustomEventAssemblyNames)).GetChildren().Select(x => x.Value!);
            });

            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hyprsoft Webhooks API", Version = "v1" });
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
            });

            if (!builder.Environment.IsEnvironment("UnitTest"))
            {
                builder.Services.Configure<WebhooksPingWorkerOptions>(builder.Configuration);
                builder.Services.AddHostedService<WebhooksPingWorker>();
            }

            var app = builder.Build();

            if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("UnitTest"))
                TelemetryDebugWriter.IsTracingDisabled = true;

            if (!builder.Environment.IsDevelopment())
                app.UseHsts();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hyprsoft Webhooks API v1"));
            app.UseWebhooksServer(app.Services);
            app.MapControllers();
            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
