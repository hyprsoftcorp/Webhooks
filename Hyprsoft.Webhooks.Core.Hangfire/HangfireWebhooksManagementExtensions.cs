using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Hyprsoft.Webhooks.Core.Management;
using System;

namespace Hyprsoft.Webhooks.Core.Hangfire
{
    public static class HangfireWebhooksManagementExtensions
    {
        public static IServiceCollection AddHangfireWebhooksServer(this IServiceCollection services) => AddHangfireWebhooksServer(services, new HangfireWebhooksManagerOptions());

        public static IServiceCollection AddHangfireWebhooksServer(this IServiceCollection services, HangfireWebhooksManagerOptions options)
        {
            if (options == null)
                throw new InvalidOperationException("The Hangfire webhooks manager options are missing.  Please check your configuration.");

            services.AddSingleton(options.HttpClientOptions);
            services.AddDbContext<WebhooksDbContext>(provider => provider.UseSqlServer(options.DatabaseConnectionString));
            services.AddScoped<IHangfireWebhooksManager, HangfireWebhooksManager>();
            services.AddScoped<IWebhooksManager>(provider => provider.GetRequiredService<IHangfireWebhooksManager>());

            // TODO: Fix this hack!  Since Hangfire expects the DB to be created and EF won't create it's entities, etc. if the DBs created we force EF to create the DB below before Hangfire takes over.
            var builder = new DbContextOptionsBuilder<WebhooksDbContext>();
            builder.UseSqlServer(options.DatabaseConnectionString);
            using (var db = new WebhooksDbContext(builder.Options)) { }

            services.AddHangfire(configuration =>
            {
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
            app.UseHangfireServer();
            // TODO: We need to allow a custom dashboard path and Hangfire authorization.
            app.UseHangfireDashboard("/hangfire", new DashboardOptions { DashboardTitle = "Webhooks Dashboard", Authorization = new[] { new HangfireAuthorizationFilter() } });

            return app;
        }
    }
}
