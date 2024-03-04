using AspNetCore.Authentication.ApiKey;
using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.AspNetCore
{
    public static class WebhooksAuthenticationExtensions
    {
        public static IServiceCollection AddWebhooksAuthentication(this IServiceCollection services) => AddWebhooksAuthentication(services, new WebhooksAuthenticationOptions());

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

        public static IServiceCollection AddWebhooksAuthentication(this IServiceCollection services, Action<WebhooksAuthenticationOptions> configure)
        {
            var options = new WebhooksAuthenticationOptions();
            configure.Invoke(options);

            return AddWebhooksAuthentication(services, options);
        }
    }
}
