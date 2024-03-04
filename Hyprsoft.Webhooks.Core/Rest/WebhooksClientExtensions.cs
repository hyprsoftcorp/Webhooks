using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hyprsoft.Webhooks.Core.Rest
{
    public static class WebhooksClientExtensions
    {
        public static IServiceCollection AddWebhooksClient(this IServiceCollection services) => AddWebhooksClient(services, new WebhooksHttpClientOptions());

        public static IServiceCollection AddWebhooksClient(this IServiceCollection services, WebhooksHttpClientOptions options)
        {
            if (options is null)
                throw new InvalidOperationException("The webhooks HTTP client options are missing.  Please check your configuration.");

            services.AddOptions<WebhooksHttpClientOptions>()
                .Configure(addOptions =>
                {
                    addOptions.ApiKey = options.ApiKey;
                    addOptions.RequestTimeout = options.RequestTimeout;
                    addOptions.ServerBaseUri = options.ServerBaseUri;
                });

            services.AddTransient<IWebhooksClient, WebhooksClient>();

            return services;
        }

        public static IServiceCollection AddWebhooksClient(this IServiceCollection services, Action<WebhooksHttpClientOptions> configure)
        {
            var options = new WebhooksHttpClientOptions();
            configure.Invoke(options);

            return AddWebhooksClient(services, options);
        }
    }
}
