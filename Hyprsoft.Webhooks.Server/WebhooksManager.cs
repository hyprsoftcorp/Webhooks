using Hyprsoft.Webhooks.Client;
using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Events;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serialize.Linq.Nodes;
using System.Linq.Expressions;

namespace Hyprsoft.Webhooks.Server
{
    public interface IWebhooksManager : IDisposable
    {
        IEnumerable<Subscription> Subscriptions { get; }

        Task SubscribeAsync(string eventName, Uri webhookUri, ExpressionNode? filter = null);

        Task UnsubscribeAsync(string eventName, Uri webhookUri);

        Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent;

        Task DispatchAsync<TEvent>(Uri webhookUri, TEvent @event) where TEvent : WebhookEvent;

        Task<WebhooksHealthSummary> GetHealthSummaryAsync(TimeSpan period);
    }

    internal abstract class WebhooksManager : IWebhooksManager, IDisposable
    {
        #region Fields

        private bool _isDisposed;
        private readonly IWebhooksRepository _database;
        private readonly WebhooksHttpClient _httpClient;

        #endregion

        #region Constructors

        public WebhooksManager(IWebhooksRepository database, IOptions<WebhooksHttpClientOptions> options)
        {
            _database = database;
            Options = options.Value;
            _httpClient = new WebhooksHttpClient(Options.ApiKey)
            {
                BaseAddress = Options.ServerBaseUri,
                Timeout = Options.RequestTimeout
            };
        }

        #endregion

        #region Properties

        public WebhooksHttpClientOptions Options { get; }

        public IEnumerable<Subscription> Subscriptions => _database.Subscriptions.OrderBy(x => x.CreatedUtc);

        #endregion

        #region Methods

        public async Task SubscribeAsync(string eventName, Uri webhookUri, ExpressionNode? filter = null)
        {
            var node = filter is null ? null : JsonConvert.SerializeObject(filter, WebhooksGlobalConfiguration.JsonSerializerSettings);
            var subscription = _database.Subscriptions.FirstOrDefault(s => s.EventName == eventName && s.WebhookUri == webhookUri);
            if (subscription is null)
            {
                subscription = new Subscription
                {
                    EventName = eventName,
                    WebhookUri = webhookUri,
                    FilterExpression = node,
                    Filter = filter?.ToString()
                };
            }
            else
            {
                subscription.IsActive = true;
                subscription.CreatedUtc = DateTime.UtcNow;
                subscription.FilterExpression = node;
                subscription.Filter = filter?.ToString();
            }

            await _database.UpsertSubscriptionAsync(subscription);
            await _database.AddAuditAsync(new Audit
            {
                EventName = eventName,
                AuditType = AuditType.Subscribe,
                WebhookUri = webhookUri
            });
        }

        public async Task UnsubscribeAsync(string eventName, Uri webhookUri)
        {
            var subscription = _database.Subscriptions.FirstOrDefault(s => s.EventName == eventName && s.WebhookUri == webhookUri);
            if (subscription is null)
                return;

            await _database.RemoveSubscriptionAsync(subscription);
            await _database.AddAuditAsync(new Audit
            {
                EventName = eventName,
                AuditType = AuditType.Unsubscribe,
                WebhookUri = webhookUri
            });
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent
        {
            await _database.AddAuditAsync(new Audit
            {
                EventName = @event.GetType().FullName!,
                AuditType = AuditType.Publish,
                Payload = JsonConvert.SerializeObject(@event)
            });

            foreach (var subscription in _database.Subscriptions.Where(s => s.EventName == @event.GetType().FullName && s.IsActive).ToList())
            {
                var shouldPublish = true;
                if (!String.IsNullOrWhiteSpace(subscription.FilterExpression))
                {
                    var node = JsonConvert.DeserializeObject<ExpressionNode>(subscription.FilterExpression, WebhooksGlobalConfiguration.JsonSerializerSettings);
                    if (node?.ToExpression() is not LambdaExpression expression)
                        throw new WebhookException($"Invalid webhook predicate expression for event '{@event.GetType().FullName}'.");

                    shouldPublish = (bool)(expression.Compile().DynamicInvoke(@event) ?? false);
                }
                if (shouldPublish)
                    await OnPublishAsync<TEvent>(subscription, @event);
            }   // for each subscription
        }

        public async Task DispatchAsync<TEvent>(Uri webhookUri, TEvent @event) where TEvent : WebhookEvent
        {
            var audit = new Audit
            {
                EventName = @event.GetType().FullName!,
                AuditType = AuditType.Dispatch,
                Payload = JsonConvert.SerializeObject(@event),
                WebhookUri = webhookUri
            };
            try
            {
                var response = await _httpClient.PostAsync(webhookUri, new WebhookContent(@event)).ConfigureAwait(false);
                await _httpClient.ValidateResponseAsync(response, "Dispatch failed.");
                await _database.AddAuditAsync(audit);
            }
            catch (Exception ex)
            {
                audit.Error = ex.Message.Length > 1024 ? ex.Message[..1024] : ex.Message;
                await _database.AddAuditAsync(audit);
                throw;
            }
        }

        public Task<WebhooksHealthSummary> GetHealthSummaryAsync(TimeSpan period)
        {
            var startDateUtc = DateTime.UtcNow - period;
            var summary = new WebhooksHealthSummary
            {
                PublishIntervalMinutes = (int)period.TotalMinutes,
                SuccessfulWebhooks = [.. _database.Audits
                            .Where(x => x.AuditType == AuditType.Dispatch && x.CreatedUtc >= startDateUtc && String.IsNullOrWhiteSpace(x.Error))
                            .GroupBy(x => x.EventName)
                            .Select(x => new WebhooksHealthSummary.SuccessfulWebhook
                            {
                                EventName = x.Key,
                                Count = x.Count()
                            }).OrderBy(x => x.EventName)],
                FailedWebhooks = [.. _database.Audits
                            .Where(x => x.AuditType == AuditType.Dispatch && x.CreatedUtc >= startDateUtc && !String.IsNullOrWhiteSpace(x.Error))
                            .GroupBy(x => new { x.EventName, x.WebhookUri, x.Error })
                            .Select(x => new WebhooksHealthSummary.FailedWebook
                            {
                                EventName = x.Key.EventName,
                                WebhookUri = x.Key.WebhookUri,
                                Error = x.Key.Error!,
                                Count = x.Count()
                            }).OrderBy(x => x.EventName)]
            };

            return Task.FromResult(summary);
        }

        protected virtual Task OnPublishAsync<TEvent>(Subscription subscription, TEvent @event) where TEvent : WebhookEvent => DispatchAsync(subscription.WebhookUri, @event);

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                _httpClient?.Dispose();
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
