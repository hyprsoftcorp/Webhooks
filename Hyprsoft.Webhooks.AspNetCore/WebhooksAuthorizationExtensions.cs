using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hyprsoft.Webhooks.AspNetCore
{
    public static class WebhooksAuthorizationExtensions
    {
        public static IServiceCollection AddWebhooksAuthorization(this IServiceCollection services) => AddWebhooksAuthorization(services, new WebhooksAuthorizationOptions());

        public static IServiceCollection AddWebhooksAuthorization(this IServiceCollection services, WebhooksAuthorizationOptions options)
        {
            if (options == null)
                throw new InvalidOperationException("The webhooks autorization options are missing.  Please check your configuration.");

            services.AddSingleton(options);
            
            return services;
        }

        public static IServiceCollection AddWebhooksAuthorization(this IServiceCollection services, Action<WebhooksAuthorizationOptions> configure)
        {
            var options = new WebhooksAuthorizationOptions();
            configure.Invoke(options);

            return AddWebhooksAuthorization(services, options: options);
        }

        public static IApplicationBuilder UseWebhooksAuthorization(this IApplicationBuilder builder) => builder.UseMiddleware<WebhooksAuthorizationMiddleware>();
    }
}
