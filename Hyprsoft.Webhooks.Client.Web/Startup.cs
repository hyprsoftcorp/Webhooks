using Hyprsoft.Webhooks.AspNetCore;
using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.AspNetCore.Builder;
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
            services.Configure<WebhooksWorkerOptions>(Configuration);
            services.AddWebhooksClient(options => options.ServerBaseUri = Configuration.GetValue<System.Uri>(nameof(WebhooksHttpClientOptions.ServerBaseUri)));
            services.AddWebhooksAuthorization();
            services.AddControllers();
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
