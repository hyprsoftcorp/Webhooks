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

        public WebhooksHealthWorker(ILogger<WebhooksHealthWorker> logger, IOptions<WebhooksHealthWorkerOptions> options, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            Options = options.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public WebhooksHealthWorkerOptions Options { get; }

        #endregion

        #region Methods

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                _logger.LogInformation($"Webhooks Health Worker Settings: Interval: {Options.PublishHealthInterval}");

                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var manager = scope.ServiceProvider.GetRequiredService<IWebhooksManager>();
                    var summary = await manager.GetHealthSummaryAsync(Options.PublishHealthInterval);
                    await manager.PublishAsync(new WebhooksHealthEvent { Summary = summary });
                    await Task.Delay(Options.PublishHealthInterval, stoppingToken);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
            }
        }

        #endregion
    }
}
