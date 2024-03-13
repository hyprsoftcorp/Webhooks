using Hyprsoft.Webhooks.Events;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Hyprsoft.Webhooks.Server.Web
{
    public class WebhooksPingWorker : BackgroundService
    {
        #region Fields

        private readonly ILogger<WebhooksPingWorker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        #endregion

        #region Constructors

        public WebhooksPingWorker(ILogger<WebhooksPingWorker> logger, IOptions<WebhooksPingWorkerOptions> options, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            Options = options.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public WebhooksPingWorkerOptions Options { get; }

        #endregion

        #region Methods

        public override Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Webhooks Ping Worker Settings: Interval: {publishPingEventInterval}", Options.PublishPingEventInterval);
            return base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var manager = scope.ServiceProvider.GetRequiredService<IWebhooksManager>();
                    var @event = new PingWebhookEvent();
                    
                    _logger.LogInformation("Publishing event '{fullName}' with payload '{payload}'.", @event.GetType().FullName, JsonConvert.SerializeObject(@event));
                    await manager.PublishAsync(@event);
                    await Task.Delay(Options.PublishPingEventInterval, stoppingToken);
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
