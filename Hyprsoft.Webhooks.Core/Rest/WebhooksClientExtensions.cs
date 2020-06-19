﻿using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hyprsoft.Webhooks.Core.Rest
{
    public static class WebhooksClientExtensions
    {
        public static IServiceCollection AddWebhooksClient(this IServiceCollection services) => AddWebhooksClient(services, new WebhooksHttpClientOptions());

        public static IServiceCollection AddWebhooksClient(this IServiceCollection services, WebhooksHttpClientOptions options)
        {
            if (options == null)
                throw new InvalidOperationException("The webhooks HTTP client options are missing.  Please check your configuration.");

            services.AddSingleton(options);
            services.AddTransient<IWebhooksClient, WebhooksClient>();

            return services;
        }

        public static IServiceCollection AddWebhooksClient(this IServiceCollection services, Action<WebhooksHttpClientOptions> configure = null)
        {
            var options = new WebhooksHttpClientOptions();
            configure.Invoke(options);

            return AddWebhooksClient(services, options);
        }
    }
}
