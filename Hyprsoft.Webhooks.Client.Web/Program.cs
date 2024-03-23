using Hyprsoft.Webhooks.Core;

namespace Hyprsoft.Webhooks.Client.Web
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var apiKey = builder.Configuration.GetValue(nameof(WebhooksAuthenticationOptions.ApiKey), WebhooksGlobalConfiguration.DefaultApiKey)!;

            builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            builder.Services.AddApiVersioning(options => options.AssumeDefaultVersionWhenUnspecified = true);
            builder.Services.AddWebhooksAuthentication(options => options.ApiKey = apiKey);
            builder.Services.AddWebhooksClient(options =>
            {
                options.ServerBaseUri = builder.Configuration.GetValue(nameof(WebhooksHttpClientOptions.ServerBaseUri), WebhooksHttpClientOptions.DefaultServerBaseUri)!;
                options.ApiKey = apiKey;
            });
            builder.Services.AddSwaggerGen();

            if (!builder.Environment.IsEnvironment("UnitTest"))
            {
                var workerOptions = new WebhooksWorkerOptions();

                builder.Services.Configure<WebhooksWorkerOptions>(builder.Configuration);
                builder.Configuration.Bind(workerOptions);
                builder.Services.AddHostedService<WebhooksWorker>();
            }

            var app = builder.Build();

            if (!builder.Environment.IsDevelopment())
                app.UseHsts();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseWebhooksAuthentication();

            app.Run();
        }
    }
}
