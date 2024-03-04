using Hyprsoft.Webhooks.Core.Events;
using Hyprsoft.Webhooks.Core.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Server.Web
{
    public class WebhooksHealthWorker : BackgroundService
    {
        #region Fields

        private readonly ILogger<WebhooksHealthWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        #endregion

        #region Constructors

        static WebhooksHealthWorker()
        {
            ServerStartDateUtc = DateTime.UtcNow;
        }

        public WebhooksHealthWorker(ILogger<WebhooksHealthWorker> logger, IOptions<WebhooksHealthWorkerOptions> options, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            Options = options.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public WebhooksHealthWorkerOptions Options { get; }

        public static DateTime ServerStartDateUtc { get; }

        #endregion

        #region Methods

        public override Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Webhooks Health Worker Settings: Interval: {interval}", Options.PublishHealthEventInterval);
            return base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var manager = scope.ServiceProvider.GetRequiredService<IWebhooksManager>(); // Singleton lifetime
                    var summary = await manager.GetHealthSummaryAsync(Options.PublishHealthEventInterval);
                    summary.ServerStartDateUtc = ServerStartDateUtc;
                    await manager.PublishAsync(new WebhooksHealthEvent { Summary = summary });
                    await Task.Delay(Options.PublishHealthEventInterval, stoppingToken);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError("{message}", ex.Message);
            }
        }

        #endregion
    }
}
