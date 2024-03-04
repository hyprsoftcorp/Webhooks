using Hangfire;
using Hyprsoft.Webhooks.Core.Management;
using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hyprsoft.Webhooks.Core.Hangfire
{
    public static class HangfireWebhooksManagementExtensions
    {
        public static IServiceCollection AddHangfireWebhooksServer(this IServiceCollection services) => AddHangfireWebhooksServer(services, new HangfireWebhooksManagerOptions());

        public static IServiceCollection AddHangfireWebhooksServer(this IServiceCollection services, HangfireWebhooksManagerOptions options)
        {
            if (options is null)
                throw new InvalidOperationException("The Hangfire webhooks manager options are missing.  Please check your configuration.");

            services.AddOptions<HangfireWebhooksManagerOptions>()
                .Configure(addOptions =>
                {
                    addOptions.DatabaseConnectionString = options.DatabaseConnectionString;
                    addOptions.HttpClientOptions = options.HttpClientOptions;
                    addOptions.UseInMemoryDatastore = options.UseInMemoryDatastore;
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
                if (options.UseInMemoryDatastore)
                    provider.UseInMemoryDatabase(WebhooksDbContext.WebhooksDbName);
                else
                    provider.UseSqlServer(options.DatabaseConnectionString);
            });
            services.AddScoped<IWebhooksStorageProvider, SqlServerWebhooksStorageProvider>();
            services.AddScoped<IHangfireWebhooksManager, HangfireWebhooksManager>();
            services.AddScoped<IWebhooksManager>(provider => provider.GetRequiredService<IHangfireWebhooksManager>());

            // TODO: Fix this hack!  
            // 1. Hangfire expects the DB to be created.
            // 2. EF won't create it's entities, etc. if the DB is created.
            // So we force EF to create the DB below before Hangfire takes over.
            var builder = new DbContextOptionsBuilder<WebhooksDbContext>();
            if (options.UseInMemoryDatastore)
                builder.UseInMemoryDatabase(WebhooksDbContext.WebhooksDbName);
            else
                builder.UseSqlServer(options.DatabaseConnectionString);
            using (var db = new WebhooksDbContext(builder.Options)) { }

            services.AddHangfire(configuration =>
            {
                if (options.UseInMemoryDatastore)
                    configuration.UseInMemoryStorage();
                else
                    configuration.UseSqlServerStorage(options.DatabaseConnectionString);
                configuration.UseSerializerSettings(WebhooksGlobalConfiguration.JsonSerializerSettings);
            });
            services.AddHangfireServer();

            return services;
        }

        public static IServiceCollection AddHangfireWebhooksServer(this IServiceCollection services, Action<HangfireWebhooksManagerOptions> configure)
        {
            var options = new HangfireWebhooksManagerOptions();
            configure.Invoke(options);

            return AddHangfireWebhooksServer(services, options);
        }

        public static IApplicationBuilder UseHangfireWebhooksServer(this IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            GlobalConfiguration.Configuration.UseActivator(new HangfireActivator(serviceProvider));
            // TODO: We need to allow a custom dashboard path and Hangfire authorization.
            app.UseHangfireDashboard("/hangfire", new DashboardOptions { DashboardTitle = "Webhooks Dashboard", Authorization = new[] { new HangfireAuthorizationFilter() } });

            return app;
        }
    }
}
