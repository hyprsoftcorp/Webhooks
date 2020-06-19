using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Hyprsoft.Webhooks.Core.Events;
using Hyprsoft.Webhooks.Core.Rest;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Hyprsoft.Webhooks.Client.Web.Controllers;

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation($"Settings: Server: {Options.ServerBaseUri} |  Webhook: {Options.WebhookBaseUri} | Role: {Options.Role} | Interval: {Options.PublishInterval} | EventCount: {Options.MaxEventsToPublishPerInterval} | AutoUnsubscribe: {Options.AutoUnsubscribe}");

                if (Options.Role == WebhooksWorkerRole.Sub || Options.Role == WebhooksWorkerRole.PubSub)
                {
                    var webhookUri = new Uri($"{Options.WebhookBaseUri}webhooks/{nameof(WebhooksController.SampleCreated).ToLower()}");
                    _logger.LogInformation($"Subscribing to event '{typeof(SampleCreatedWebhookEvent).FullName}' with webhook '{webhookUri}'.");
                    await _webhooksClient.SubscribeAsync<SampleCreatedWebhookEvent>(webhookUri, (System.Linq.Expressions.Expression<Func<SampleCreatedWebhookEvent, bool>>)(x => x.SampleType > 2));

                    webhookUri = new Uri($"{Options.WebhookBaseUri}webhooks/{nameof(WebhooksController.SampleIsActiveChanged).ToLower()}");
                    _logger.LogInformation($"Subscribing to event '{typeof(SampleIsActiveChangedWebhookEvent).FullName}' with webhook '{webhookUri}'.");
                    await _webhooksClient.SubscribeAsync<SampleIsActiveChangedWebhookEvent>(webhookUri);

                    webhookUri = new Uri($"{Options.WebhookBaseUri}webhooks/{nameof(WebhooksController.SampleDeleted).ToLower()}");
                    _logger.LogInformation($"Subscribing to event '{typeof(SampleDeletedWebhookEvent).FullName}' with webhook '{webhookUri}'.");
                    await _webhooksClient.SubscribeAsync<SampleDeletedWebhookEvent>(webhookUri, (System.Linq.Expressions.Expression<Func<SampleDeletedWebhookEvent, bool>>)(x => x.SampleType > 2));
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (Options.Role == WebhooksWorkerRole.Pub || Options.Role == WebhooksWorkerRole.PubSub)
                    {
                        for (int i = 0; i < _random.Next(1, Options.MaxEventsToPublishPerInterval + 1); i++)
                        {
                            WebhookEvent @event = _random.Next(1, 4) switch
                            {
                                1 => new SampleCreatedWebhookEvent
                                {
                                    SampleId = _random.Next(),
                                    SampleType = _random.Next(1, 6),
                                    UserId = _random.Next(),
                                    ReferenceId = _random.Next()
                                },
                                2 => new SampleIsActiveChangedWebhookEvent
                                {
                                    SampleId = _random.Next(),
                                    SampleType = _random.Next(1, 6),
                                    UserId = _random.Next(),
                                    IsActive = Convert.ToBoolean(_random.Next(0, 2))
                                },
                                3 => new SampleDeletedWebhookEvent
                                {
                                    SampleId = _random.Next(),
                                    SampleType = _random.Next(1, 6),
                                    UserId = _random.Next()
                                },
                                _ => throw new NotImplementedException(),
                            };
                            _logger.LogInformation($"Publishing event '{@event.GetType().FullName}' with payload '{JsonConvert.SerializeObject(@event)}'.");
                            await _webhooksClient.PublishAsync(@event);
                        }   // publish event for loop
                    }   // publish events?

                    await Task.Delay(Options.PublishInterval, stoppingToken);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            finally
            {
                if (Options.AutoUnsubscribe && (Options.Role == WebhooksWorkerRole.Sub || Options.Role == WebhooksWorkerRole.PubSub))
                {
                    var webhookUri = new Uri($"{Options.WebhookBaseUri}webhooks/{nameof(WebhooksController.SampleCreated).ToLower()}");
                    _logger.LogInformation($"Unsubscribing from event '{typeof(SampleCreatedWebhookEvent).FullName}' with webhook '{webhookUri}'.");
                    await _webhooksClient.UnsubscribeAsync<SampleCreatedWebhookEvent>(webhookUri);

                    webhookUri = new Uri($"{Options.WebhookBaseUri}webhooks/{nameof(WebhooksController.SampleIsActiveChanged).ToLower()}");
                    _logger.LogInformation($"Unsubscribing from event '{typeof(SampleIsActiveChangedWebhookEvent)}' with webhook '{webhookUri}'.");
                    await _webhooksClient.UnsubscribeAsync<SampleIsActiveChangedWebhookEvent>(webhookUri);

                    webhookUri = new Uri($"{Options.WebhookBaseUri}webhooks/{nameof(WebhooksController.SampleDeleted).ToLower()}");
                    _logger.LogInformation($"Unsubscribing from event '{typeof(SampleDeletedWebhookEvent).FullName}' with webhook '{webhookUri}'.");
                    await _webhooksClient.UnsubscribeAsync<SampleDeletedWebhookEvent>(webhookUri);
                }
            }
        }

        #endregion
    }
}
