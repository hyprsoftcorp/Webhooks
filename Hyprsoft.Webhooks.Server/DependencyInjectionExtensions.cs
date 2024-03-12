﻿using Hangfire;
using Hyprsoft.Webhooks.Client;
using Hyprsoft.Webhooks.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hyprsoft.Webhooks.Server
{
    public static class DependencyInjectionExtensions
    {
        private const string CorsPolicyName = "WebhooksCorsPolicy";

        public static IServiceCollection AddWebhooksServer(this IServiceCollection services) => AddWebhooksServer(services, new WebhooksManagerOptions());

        public static IServiceCollection AddWebhooksServer(this IServiceCollection services, Action<WebhooksManagerOptions> configure)
        {
            var options = new WebhooksManagerOptions();
            configure.Invoke(options);

            return AddWebhooksServer(services, options);
        }

        public static IServiceCollection AddWebhooksServer(this IServiceCollection services, WebhooksManagerOptions options)
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
                policy.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }));

            services.AddScoped<IWebhooksRepository, WebhooksRepository>();
            services.AddScoped<IWebhooksManager, HangfireWebhooksManager>();

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

            return services;
        }

        public static IApplicationBuilder UseWebhooksServer(this IApplicationBuilder app, IServiceProvider serviceProvider)
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