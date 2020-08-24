using Hyprsoft.Webhooks.Core.Events;
using Hyprsoft.Webhooks.Core.Rest;
using Newtonsoft.Json;
using Serialize.Linq.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Management
{
    public interface IWebhooksManager : IDisposable
    {
        IEnumerable<Subscription> Subscriptions { get; }

        Task SubscribeAsync(string eventName, Uri webhookUri, ExpressionNode filter = null);

        Task UnsubscribeAsync(string eventName, Uri webhookUri);

        Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent;

        Task<WebhooksHealthSummary> GetHealthSummaryAsync(TimeSpan period);
    }

    public abstract class WebhooksManager : IDisposable
    {
        #region Fields

        private bool _isDisposed;
        private readonly IWebhooksStorageProvider _storageProvider;

        #endregion

        #region Constructors

        public WebhooksManager(IWebhooksStorageProvider storageProvider, WebhooksHttpClientOptions options)
        {
            _storageProvider = storageProvider;
            Options = options;
            HttpClient = new WebhooksHttpClient(Options.PayloadSigningSecret)
            {
                BaseAddress = Options.ServerBaseUri,
                Timeout = Options.RequestTimeout
            };
        }

        #endregion

        #region Properties

        public WebhooksHttpClientOptions Options { get; }

        public IEnumerable<Subscription> Subscriptions => _storageProvider.Subscriptions.OrderBy(x => x.CreatedUtc);

        protected WebhooksHttpClient HttpClient { get; }

        #endregion

        #region Methods

        public async Task SubscribeAsync(string eventName, Uri webhookUri, ExpressionNode filter = null)
        {
            var node = filter == null ? null : JsonConvert.SerializeObject(filter, WebhooksGlobalConfiguration.JsonSerializerSettings);
            var subscription = _storageProvider.Subscriptions.FirstOrDefault(s => s.EventName == eventName && s.WebhookUri == webhookUri);
            if (subscription == null)
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
            await _storageProvider.UpsertSubscriptionAsync(subscription);
        }

        public async Task UnsubscribeAsync(string eventName, Uri webhookUri)
        {
            var subscription = _storageProvider.Subscriptions.FirstOrDefault(s => s.EventName == eventName && s.WebhookUri == webhookUri);
            if (subscription != null)
                await _storageProvider.RemoveSubscriptionAsync(subscription);
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent
        {
            foreach (var subscription in _storageProvider.Subscriptions.Where(s => s.EventName == @event.GetType().FullName && s.IsActive).ToList())
            {
                var shouldPublish = true;
                if (!String.IsNullOrWhiteSpace(subscription.FilterExpression))
                {
                    var node = JsonConvert.DeserializeObject<ExpressionNode>(subscription.FilterExpression, WebhooksGlobalConfiguration.JsonSerializerSettings);
                    if (!(node.ToExpression() is LambdaExpression expression))
                        throw new WebhookException($"Invalid webhook predicate expression for event '{@event.GetType().FullName}'.");

                    shouldPublish = (bool)expression.Compile().DynamicInvoke(@event);
                }
                if (shouldPublish)
                {
                    var audit = new Audit
                    {
                        EventName = subscription.EventName,
                        Filter = subscription.Filter,
                        Payload = JsonConvert.SerializeObject(@event),
                        WebhookUri = subscription.WebhookUri
                    };
                    try
                    {
                        await OnPublishAsync<TEvent>(subscription, @event);
                        await _storageProvider.AddAuditAsync(audit);
                    }
                    catch (Exception ex)
                    {
                        audit.Error = ex.ToString();
                        await _storageProvider.AddAuditAsync(audit);
                        throw;
                    }
                }
            }   // should publish?
        }

        public async Task DispatchAsync<TEvent>(Uri webhookUri, TEvent @event) where TEvent : WebhookEvent
        {
            var response = await HttpClient.PostAsync(webhookUri, new WebhookContent(@event)).ConfigureAwait(false);
            await HttpClient.ValidateResponseAsync(response, "Dispatch failed.");
        }

        public Task<WebhooksHealthSummary> GetHealthSummaryAsync(TimeSpan period)
        {
            var summary = new WebhooksHealthSummary
            {
                StartDateUtc = DateTime.UtcNow - period,
                EndDateUtc = DateTime.UtcNow,
            };
            summary.SuccessfulWebhooks = _storageProvider.Audits
                            .Where(x => x.CreatedUtc >= summary.StartDateUtc && String.IsNullOrWhiteSpace(x.Error))
                            .GroupBy(x => x.EventName)
                            .Select(x => new WebhooksHealthSummary.SuccessfulWebhook
                            {
                                EventName = x.Key,
                                Count = x.Count()
                            }).OrderBy(x => x.EventName)
                            .ToList();
            summary.FailedWebhooks = _storageProvider.Audits
                            .Where(x => x.CreatedUtc >= summary.StartDateUtc && !String.IsNullOrWhiteSpace(x.Error))
                            .GroupBy(x => new { x.EventName, x.WebhookUri, x.Error })
                            .Select(x => new WebhooksHealthSummary.FailedWebook
                            {
                                EventName = x.Key.EventName,
                                WebhookUri = x.Key.WebhookUri,
                                Error = x.Key.Error,
                                Count = x.Count()
                            }).OrderBy( x=> x.EventName)
                            .ToList();

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
                HttpClient?.Dispose();
            }

            _isDisposed = true;
        }

        public void Dispose() => Dispose(true);

        #endregion
    }
}
