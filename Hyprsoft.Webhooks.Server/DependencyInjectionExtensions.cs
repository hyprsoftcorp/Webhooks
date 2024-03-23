using Hangfire;
using Hyprsoft.Webhooks.Client;
using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Hyprsoft.Webhooks.Server
{
    public static class DependencyInjectionExtensions
    {
        private const string CorsPolicyName = "WebhooksCorsPolicy";

        public static IServiceCollection AddWebhooksServer(this IServiceCollection services, IConfigurationManager configuration) => AddWebhooksServer(services, configuration, new WebhooksManagerOptions());

        public static IServiceCollection AddWebhooksServer(this IServiceCollection services, IConfigurationManager configuration, Action<WebhooksManagerOptions> configure)
        {
            var options = new WebhooksManagerOptions();
            configure.Invoke(options);

            return AddWebhooksServer(services, configuration, options);
        }

        public static IServiceCollection AddWebhooksServer(this IServiceCollection services, IConfigurationManager configuration, WebhooksManagerOptions options)
        {
            if (options is null)
                throw new InvalidOperationException("The Hangfire webhooks manager options are missing.  Please check your configuration.");

            var useInMemoryDatastore = string.IsNullOrWhiteSpace(options.DatabaseConnectionString);

            services.AddOptions<WebhooksManagerOptions>()
                .Configure(addOptions =>
                {
                    addOptions.DatabaseConnectionString = options.DatabaseConnectionString;
                    addOptions.HttpClientOptions = options.HttpClientOptions;
                });

            services.AddOptions<WebhooksHttpClientOptions>()
                .Configure(addOptions =>
                {
                    addOptions.ApiKey = options.HttpClientOptions.ApiKey;
                    addOptions.RequestTimeout = options.HttpClientOptions.RequestTimeout;
                    addOptions.ServerBaseUri = options.HttpClientOptions.ServerBaseUri;
                });

            services.AddDbContext<WebhooksDbContext>(provider =>
            {
                if (useInMemoryDatastore)
                    provider.UseInMemoryDatabase(WebhooksGlobalConfiguration.WebhooksDbName);
                else
                    provider.UseSqlServer(options.DatabaseConnectionString);
            });

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.AddCors(configure => configure.AddPolicy(CorsPolicyName, policy =>
            {
                policy.WithOrigins(configuration.GetSection("corsOrigins").Get<string[]>() ?? [])
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }));

            services.TryAddScoped<IWebhooksRepository, WebhooksRepository>();
            services.TryAddTransient<WebhooksMessageHandler>(services => new WebhooksMessageHandler(services.GetRequiredService<IOptions<WebhooksHttpClientOptions>>().Value.ApiKey));
            services.AddHttpClient<IWebhooksManager, HangfireWebhooksManager>()
                .AddHttpMessageHandler<WebhooksMessageHandler>();

            // TODO: Fix this hack!  
            // 1. Hangfire expects the DB to be created.
            // 2. EF won't create it's entities, etc. if the DB is created.
            // So we force EF to create the DB below before Hangfire takes over.
            var builder = new DbContextOptionsBuilder<WebhooksDbContext>();
            if (useInMemoryDatastore)
                builder.UseInMemoryDatabase(WebhooksGlobalConfiguration.WebhooksDbName);
            else
                builder.UseSqlServer(options.DatabaseConnectionString);
            using (var db = new WebhooksDbContext(builder.Options)) { }

            services.AddHangfire(configuration =>
            {
                if (useInMemoryDatastore)
                    configuration.UseInMemoryStorage();
                else
                    configuration.UseSqlServerStorage(options.DatabaseConnectionString);
                configuration.UseSerializerSettings(WebhooksGlobalConfiguration.JsonSerializerSettings);
            });
            services.AddHangfireServer();

            foreach (var customEventAssemblyName in options.CustomEventAssemblyNames)
            {
                var assemblyFilename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, customEventAssemblyName);
                if (File.Exists(assemblyFilename))
                {
                    var assembly = Assembly.LoadFrom(assemblyFilename);
                    if (assembly is null)
                        continue;

                    var eventType = assembly.GetExportedTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(WebhookEvent)));
                    if (eventType is not null && !string.IsNullOrWhiteSpace(eventType.FullName))
                    {
                        var _ = assembly.CreateInstance(eventType.FullName);
                    }
                }
            }

            return services;
        }

        public static WebApplication UseWebhooksServer(this WebApplication app, IServiceProvider serviceProvider)
        {
            GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));
            // TODO: We need to allow a custom dashboard path and Hangfire authorization.
            app.UseHangfireDashboard("/hangfire", new DashboardOptions { DashboardTitle = "Webhooks Dashboard", Authorization = new[] { new HangfireAuthorizationFilter() } });
            app.UseCors(CorsPolicyName);
            app.UseAuthentication();
            app.UseAuthorization();

            return app;
        }
    }
}
