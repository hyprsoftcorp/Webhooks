using Hyprsoft.Webhooks.AspNetCore;
using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Hyprsoft.Webhooks.Client.Web
{
    public class Startup
    {
        #region Constructors

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }
        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        #endregion

        #region Methods

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
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.TypeNameHandling = WebhooksGlobalConfiguration.JsonSerializerSettings.TypeNameHandling);
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.Conventions.Add(new VersionByNamespaceConvention());
            });
            if (!Environment.IsEnvironment("UnitTest"))
                services.AddHostedService<WebhooksWorker>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseWebhooksAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        #endregion
    }
}
