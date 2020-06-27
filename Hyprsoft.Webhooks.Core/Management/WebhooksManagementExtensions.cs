using Microsoft.Extensions.DependencyInjection;
using Hyprsoft.Webhooks.Core.Rest;
using System;

namespace Hyprsoft.Webhooks.Core.Management
{
    public static class WebhooksManagementExtensions
    {
        public static IServiceCollection AddWebhooksServer(this IServiceCollection services) => AddWebhooksServer(services, new WebhooksHttpClientOptions());

        public static IServiceCollection AddWebhooksServer(this IServiceCollection services, WebhooksHttpClientOptions options)
        {
            if (options == null)
                throw new InvalidOperationException("The webhooks HTTP client options are missing.  Please check your configuration.");

            services.AddSingleton(options);
            services.AddSingleton<IWebhooksStorageProvider, InMemoryWebhooksStorageProvider>();
            services.AddSingleton<IWebhooksManager, InMemoryWebhooksManager>();

            return services;
        }

        public static IServiceCollection AddWebhooksServer(this IServiceCollection services, Action<WebhooksHttpClientOptions> configure)
        {
            var options = new WebhooksHttpClientOptions();
            configure.Invoke(options);

            return AddWebhooksServer(services, options: options);
        }
    }
}
