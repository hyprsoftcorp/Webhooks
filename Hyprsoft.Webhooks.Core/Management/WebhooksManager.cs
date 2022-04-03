using Hyprsoft.Webhooks.Core.Events;
using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.Extensions.Options;
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
        private readonly WebhooksHttpClient _httpClient;

        #endregion

        #region Constructors

        public WebhooksManager(IWebhooksStorageProvider storageProvider, IOptions<WebhooksHttpClientOptions> options)
        {
            _storageProvider = storageProvider;
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

        public IEnumerable<Subscription> Subscriptions => _storageProvider.Subscriptions.OrderBy(x => x.CreatedUtc);

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
                    await OnPublishAsync<TEvent>(subscription, @event);
            }   // for each subscription
        }

        public async Task DispatchAsync<TEvent>(Uri webhookUri, TEvent @event) where TEvent : WebhookEvent
        {
            var audit = new Audit
            {
                EventName = @event.GetType().FullName,
                Payload = JsonConvert.SerializeObject(@event),
                WebhookUri = webhookUri
            };
            try
            {
                var response = await _httpClient.PostAsync(webhookUri, new WebhookContent(@event)).ConfigureAwait(false);
                await _httpClient.ValidateResponseAsync(response, "Dispatch failed.");
                await _storageProvider.AddAuditAsync(audit);
            }
            catch (Exception ex)
            {
                audit.Error = ex.Message.Length > 1024 ? ex.Message.Substring(0, 1024) : ex.Message;
                await _storageProvider.AddAuditAsync(audit);
                throw;
            }
        }

        public Task<WebhooksHealthSummary> GetHealthSummaryAsync(TimeSpan period)
        {
            var startDateUtc = DateTime.UtcNow - period;
            var summary = new WebhooksHealthSummary
            {
                PublishIntervalMinutes = (int)period.TotalMinutes,
                SuccessfulWebhooks = _storageProvider.Audits
                            .Where(x => x.CreatedUtc >= startDateUtc && String.IsNullOrWhiteSpace(x.Error))
                            .GroupBy(x => x.EventName)
                            .Select(x => new WebhooksHealthSummary.SuccessfulWebhook
                            {
                                EventName = x.Key,
                                Count = x.Count()
                            }).OrderBy(x => x.EventName)
                            .ToList(),
                FailedWebhooks = _storageProvider.Audits
                            .Where(x => x.CreatedUtc >= startDateUtc && !String.IsNullOrWhiteSpace(x.Error))
                            .GroupBy(x => new { x.EventName, x.WebhookUri, x.Error })
                            .Select(x => new WebhooksHealthSummary.FailedWebook
                            {
                                EventName = x.Key.EventName,
                                WebhookUri = x.Key.WebhookUri,
                                Error = x.Key.Error,
                                Count = x.Count()
                            }).OrderBy(x => x.EventName)
                            .ToList()
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

        public void Dispose() => Dispose(true);

        #endregion
    }
}
