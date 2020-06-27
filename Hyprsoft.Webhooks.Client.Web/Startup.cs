using Hyprsoft.Webhooks.AspNetCore;
using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hyprsoft.Webhooks.Client.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var payloadSigningSecret = Configuration.GetValue(nameof(WebhooksAuthorizationOptions.PayloadSigningSecret), WebhooksGlobalConfiguration.DefaultPayloadSigningSecret);
            services.Configure<WebhooksWorkerOptions>(Configuration);
            services.AddWebhooksClient(options =>
            {
                options.ServerBaseUri = Configuration.GetValue(nameof(WebhooksHttpClientOptions.ServerBaseUri), WebhooksHttpClientOptions.DefaultServerBaseUri);
                options.PayloadSigningSecret = payloadSigningSecret;
            });
            services.AddWebhooksAuthorization(options => options.PayloadSigningSecret = payloadSigningSecret);
            services.AddControllers();
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.Conventions.Add(new VersionByNamespaceConvention());
            });
            services.AddHostedService<WebhooksWorker>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseWebhooksAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
