﻿using AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
[assembly: InternalsVisibleToAttribute("Hyprsoft.Webhooks.Server")]

namespace Hyprsoft.Webhooks.Client
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddWebhooksAuthentication(this IServiceCollection services) => AddWebhooksAuthentication(services, new WebhooksAuthenticationOptions());

        public static IServiceCollection AddWebhooksAuthentication(this IServiceCollection services, Action<WebhooksAuthenticationOptions> configure)
        {
            var options = new WebhooksAuthenticationOptions();
            configure.Invoke(options);

            return AddWebhooksAuthentication(services, options);
        }

        public static IServiceCollection AddWebhooksAuthentication(this IServiceCollection services, WebhooksAuthenticationOptions options)
        {
            if (options is null)
                throw new InvalidOperationException("The webhooks authentication options are missing.  Please check your configuration.");

            services.AddOptions<WebhooksAuthenticationOptions>()
                .Configure(addOptions =>
                {
                    addOptions.ApiKey = options.ApiKey;
                    addOptions.ApiUserIdentifier = options.ApiUserIdentifier;
                    addOptions.ApiUserUserName = options.ApiUserUserName;
                });

            services.AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
                .AddApiKeyInHeader(apiOptions =>
                {
                    apiOptions.Realm = "Webhooks API";
                    apiOptions.KeyName = WebhooksHttpClient.ApiKeyHeaderName;
                    apiOptions.Events = new ApiKeyEvents
                    {
                        OnValidateKey = (context) =>
                        {
                            if (string.Compare(context.ApiKey, options.ApiKey, true) != 0)
                                context.NoResult();
                            else
                            {
                                var claims = new[]
                                {
                                    new Claim(ClaimTypes.NameIdentifier, options.ApiUserIdentifier, ClaimValueTypes.Integer32, context.Options.ClaimsIssuer),
                                    new Claim(ClaimTypes.Name, options.ApiUserUserName, ClaimValueTypes.String, context.Options.ClaimsIssuer)
                                };
                                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                                context.Success();
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }

        public static IApplicationBuilder UseWebhooksAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }

        public static IServiceCollection AddWebhooksClient(this IServiceCollection services) => AddWebhooksClient(services, new WebhooksHttpClientOptions());

        public static IServiceCollection AddWebhooksClient(this IServiceCollection services, Action<WebhooksHttpClientOptions> configure)
        {
            var options = new WebhooksHttpClientOptions();
            configure.Invoke(options);

            return AddWebhooksClient(services, options);
        }

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
    }
}