using Hyprsoft.Webhooks.Client.Web.V1.Controllers;
using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Core.Events;
using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Client.Web
{
    public class WebhooksWorker : BackgroundService
    {
        #region Fields

        private readonly ILogger<WebhooksWorker> _logger;
        private readonly IWebhooksClient _webhooksClient;
        private readonly Random _random = new Random((int)DateTime.Now.Ticks);

        #endregion

        #region Constructors

        public WebhooksWorker(ILogger<WebhooksWorker> logger, IOptions<WebhooksWorkerOptions> options, IWebhooksClient webhooksClient)
        {
            _logger = logger;
            Options = options.Value;
            _webhooksClient = webhooksClient;
        }

        #endregion

        #region Properties

        public WebhooksWorkerOptions Options { get; }

        #endregion

        #region Methods

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Webhooks Worker Settings: Server: {Options.ServerBaseUri} |  Webhook: {Options.WebhooksBaseUri} | Role: {Options.Role} | AutoUnsubscribe: {Options.AutoUnsubscribe}");
            if (Options.Role == WebhooksWorkerRole.Sub || Options.Role == WebhooksWorkerRole.PubSub)
            {
                await MakeSubscriptionRequestAsync<PingWebhookEvent>(nameof(WebhooksController.Ping), true);
                await MakeSubscriptionRequestAsync<WebhooksHealthEvent>(nameof(WebhooksController.HealthSummary), true);
            }
            await base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            async Task PublishPingAsync(bool isException = false)
            {
                var @event = new PingWebhookEvent { IsException = isException };
                _logger.LogInformation($"Publishing event '{@event.GetType().FullName}' with payload '{JsonConvert.SerializeObject(@event)}'.");
                await _webhooksClient.PublishAsync(@event);
            }

            try
            {
                await Task.Delay(3000, stoppingToken);
                while (!stoppingToken.IsCancellationRequested)
                {
                    // If we aren't publishing any events then we are just a subscriber and can "delay" infinitely.
                    var delay = 0;
                    if (Options.Role == WebhooksWorkerRole.Pub || Options.Role == WebhooksWorkerRole.PubSub)
                    {
                        // Publish a ping every 1 to 60 seconds.
                        delay = _random.Next(1, 61);
                        await PublishPingAsync();

                        // Let's randomly simulate a problematic webhook.
                        if (!stoppingToken.IsCancellationRequested && _random.Next(1, 1001) == 1)
                            await PublishPingAsync(true);

                        _logger.LogInformation($"Next publish at '{DateTime.Now.AddSeconds(delay)}'.");
                    }   // publish events?

                    await Task.Delay(delay * 1_000, stoppingToken);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            if (Options.AutoUnsubscribe && (Options.Role == WebhooksWorkerRole.Sub || Options.Role == WebhooksWorkerRole.PubSub))
            {
                await MakeSubscriptionRequestAsync<PingWebhookEvent>(nameof(WebhooksController.Ping), false);
                await MakeSubscriptionRequestAsync<WebhooksHealthEvent>(nameof(WebhooksController.HealthSummary), false);
            }
            await base.StopAsync(stoppingToken);
        }

        private async Task MakeSubscriptionRequestAsync<TEvent>(string path, bool subscribe, Expression<Func<TEvent, bool>> filter = null) where TEvent : WebhookEvent
        {
            var uri = new Uri($"{Options.WebhooksBaseUri}webhooks/v{WebhooksGlobalConfiguration.LatestWebhooksApiVersion}/{path.ToLower()}");
            if (subscribe)
            {
                _logger.LogInformation($"Subscribing to event '{typeof(TEvent).FullName}' with webhook '{uri}'.");
                await _webhooksClient.SubscribeAsync<TEvent>(uri, filter);
            }
            else
            {
                _logger.LogInformation($"Unsubscribing from event '{typeof(TEvent).FullName}' with webhook '{uri}'.");
                await _webhooksClient.UnsubscribeAsync<TEvent>(uri);
            }
        }

        #endregion
    }
}
