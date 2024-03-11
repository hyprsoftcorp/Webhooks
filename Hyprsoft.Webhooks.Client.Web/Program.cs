using Hyprsoft.Webhooks.Core;

namespace Hyprsoft.Webhooks.Client.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var apiKey = builder.Configuration.GetValue(nameof(WebhooksAuthenticationOptions.ApiKey), WebhooksGlobalConfiguration.DefaultApiKey)!;
            var workerOptions = new WebhooksWorkerOptions();

            builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.TypeNameHandling = WebhooksGlobalConfiguration.JsonSerializerSettings.TypeNameHandling);
            builder.Services.AddApiVersioning(options => options.AssumeDefaultVersionWhenUnspecified = true);
            if (!builder.Environment.IsEnvironment("UnitTest"))
                builder.Services.AddHostedService<WebhooksWorker>();

            builder.Configuration.Bind(workerOptions);
            builder.Services.Configure<WebhooksWorkerOptions>(builder.Configuration);
            builder.Services.AddWebhooksAuthentication(options => options.ApiKey = apiKey);
            builder.Services.AddWebhooksClient(options =>
            {
                options.ServerBaseUri = builder.Configuration.GetValue(nameof(WebhooksHttpClientOptions.ServerBaseUri), WebhooksHttpClientOptions.DefaultServerBaseUri)!;
                options.ApiKey = apiKey;
            });

            var app = builder.Build();

            if (!builder.Environment.IsDevelopment())
                app.UseHsts();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseWebhooksAuthentication();
            app.MapControllers();

            app.Run();
        }
    }
}
