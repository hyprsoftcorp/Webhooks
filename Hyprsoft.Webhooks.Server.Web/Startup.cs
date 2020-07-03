using Hyprsoft.Webhooks.AspNetCore;
using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Core.Hangfire;
using Hyprsoft.Webhooks.Core.Management;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Hyprsoft.Webhooks.Server.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var payloadSigningSecret = Configuration.GetValue(nameof(WebhooksAuthorizationOptions.PayloadSigningSecret), WebhooksGlobalConfiguration.DefaultPayloadSigningSecret);
            services.AddWebhooksAuthorization(options => options.PayloadSigningSecret = payloadSigningSecret);
            if (Environment.IsEnvironment("UnitTest"))
                services.AddWebhooksServer(options => options.PayloadSigningSecret = payloadSigningSecret);
            else
            {
                services.AddHangfireWebhooksServer(options =>
                {
                    options.DatabaseConnectionString = Configuration.GetConnectionString(WebhooksDbContext.WebhooksDbName);
                    options.UseInMemoryDatastore = Configuration.GetValue(nameof(HangfireWebhooksManagerOptions.UseInMemoryDatastore), true);
                    options.HttpClientOptions.PayloadSigningSecret = payloadSigningSecret;
                });
            }
            services.AddControllersWithViews().AddNewtonsoftJson(options => options.SerializerSettings.TypeNameHandling = WebhooksGlobalConfiguration.JsonSerializerSettings.TypeNameHandling);
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.Conventions.Add(new VersionByNamespaceConvention());
            });
            services.AddApplicationInsightsTelemetry();
        }

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            if (Environment.IsDevelopment() || Environment.IsEnvironment("UnitTest"))
            {
                app.UseDeveloperExceptionPage();
                TelemetryDebugWriter.IsTracingDisabled = true;
            }

            app.UseStaticFiles();
            app.UseHsts();
            app.UseRouting();
            app.UseWebhooksAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}"));
            if (!Environment.IsEnvironment("UnitTest"))
                app.UseHangfireWebhooksServer(serviceProvider);
        }
    }
}
