using Hyprsoft.Webhooks.AspNetCore;
using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Core.Hangfire;
using Hyprsoft.Webhooks.Core.Management;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace Hyprsoft.Webhooks.Server.Web
{
    public class Startup
    {
        #region Constructors

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        #endregion

        #region Methods

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
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.Conventions.Add(new VersionByNamespaceConvention());
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hyprsoft Webhooks API", Version = "v1" });
                c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.XML"));
            });
            services.AddApplicationInsightsTelemetry();
            services.Configure<WebhooksHealthWorkerOptions>(Configuration);
            services.AddHostedService<WebhooksHealthWorker>();
        }

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider, IWebHostEnvironment env)
        {
            if (Environment.IsDevelopment() || Environment.IsEnvironment("UnitTest"))
            {
                app.UseDeveloperExceptionPage();
                TelemetryDebugWriter.IsTracingDisabled = true;
            }

            app.UseHsts();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }
            app.UseSwagger();
            app.UseSwaggerUI(config => config.SwaggerEndpoint("/swagger/v1/swagger.json", "Hyprsoft Webhooks API v1"));
            app.UseRouting();
            app.UseWebhooksAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}"));
            if (!Environment.IsEnvironment("UnitTest"))
                app.UseHangfireWebhooksServer(serviceProvider);
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }

        #endregion
    }
}
