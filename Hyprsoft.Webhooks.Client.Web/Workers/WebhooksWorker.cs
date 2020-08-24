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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            async Task MakeSubscriptionRequestAsync<TEvent>(string path, bool subscribe, Expression<Func<TEvent, bool>> filter = null) where TEvent : WebhookEvent
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

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                _logger.LogInformation($"Webhooks Worker Settings: Server: {Options.ServerBaseUri} |  Webhook: {Options.WebhooksBaseUri} | Role: {Options.Role} | Interval: {Options.PublishInterval} | EventCount: {Options.MaxEventsToPublishPerInterval} | AutoUnsubscribe: {Options.AutoUnsubscribe}");

                if (Options.Role == WebhooksWorkerRole.Sub || Options.Role == WebhooksWorkerRole.PubSub)
                {
                    await MakeSubscriptionRequestAsync<SampleCreatedWebhookEvent>(nameof(WebhooksController.SampleCreated), true, x => x.SampleType > 2);
                    await MakeSubscriptionRequestAsync<SampleIsActiveChangedWebhookEvent>(nameof(WebhooksController.SampleIsActiveChanged), true);
                    await MakeSubscriptionRequestAsync<SampleDeletedWebhookEvent>(nameof(WebhooksController.SampleDeleted), true, x => x.SampleType > 2);
                    await MakeSubscriptionRequestAsync<SampleExceptionWebhookEvent>(nameof(WebhooksController.SampleException), true, x => x.SampleType == 1);
                    await MakeSubscriptionRequestAsync<WebhooksHealthEvent>(nameof(WebhooksController.HealthSummary), true);
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    if (Options.Role == WebhooksWorkerRole.Pub || Options.Role == WebhooksWorkerRole.PubSub)
                    {
                        for (int i = 0; i < _random.Next(1, Options.MaxEventsToPublishPerInterval + 1); i++)
                        {
                            WebhookEvent @event = _random.Next(1, 5) switch
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
                                4 => new SampleExceptionWebhookEvent
                                {
                                    SampleId = _random.Next(),
                                    SampleType = _random.Next(1, 101) == 1 ? 1 : 2,  // Since we only suscribe to exception events with sample type = 1; 1 in 100% chance.
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
                    await MakeSubscriptionRequestAsync<SampleCreatedWebhookEvent>(nameof(WebhooksController.SampleCreated), false);
                    await MakeSubscriptionRequestAsync<SampleIsActiveChangedWebhookEvent>(nameof(WebhooksController.SampleIsActiveChanged), false);
                    await MakeSubscriptionRequestAsync<SampleDeletedWebhookEvent>(nameof(WebhooksController.SampleDeleted), false);
                    await MakeSubscriptionRequestAsync<SampleExceptionWebhookEvent>(nameof(WebhooksController.SampleException), false);
                    await MakeSubscriptionRequestAsync<WebhooksHealthEvent>(nameof(WebhooksController.HealthSummary), false);
                }
            }
        }

        #endregion
    }
}
