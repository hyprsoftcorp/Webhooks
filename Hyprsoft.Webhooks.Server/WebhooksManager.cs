using Hyprsoft.Webhooks.Client;
using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Events;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serialize.Linq.Nodes;
using System.Linq.Expressions;

namespace Hyprsoft.Webhooks.Server
{
    public interface IWebhooksManager
    {
        IEnumerable<Subscription> Subscriptions { get; }

        Task SubscribeAsync(string eventName, Uri webhookUri, ExpressionNode? filter = null);

        Task UnsubscribeAsync(string eventName, Uri webhookUri);

        Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent;

        Task DispatchAsync<TEvent>(Uri webhookUri, TEvent @event) where TEvent : WebhookEvent;

        Task<WebhooksHealthSummary> GetHealthSummaryAsync(TimeSpan period);
    }

    internal abstract class WebhooksManager : IWebhooksManager
    {
        #region Fields

        private readonly IWebhooksRepository _webhooksRepository;
        private readonly HttpClient _httpClient;

        #endregion

        #region Constructors

        public WebhooksManager(IWebhooksRepository reepository, HttpClient httpClient)
        {
            _webhooksRepository = reepository;
            _httpClient = httpClient;
        }

        #endregion

        #region Properties

        public IEnumerable<Subscription> Subscriptions => _webhooksRepository.Subscriptions.OrderBy(x => x.CreatedUtc);

        #endregion

        #region Methods

        public async Task SubscribeAsync(string eventName, Uri webhookUri, ExpressionNode? filter = null)
        {
            var node = filter is null ? null : JsonConvert.SerializeObject(filter, WebhooksGlobalConfiguration.JsonSerializerSettings);
            var subscription = _webhooksRepository.Subscriptions.FirstOrDefault(s => s.EventName == eventName && s.WebhookUri == webhookUri);
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

            await _webhooksRepository.UpsertSubscriptionAsync(subscription);
            await _webhooksRepository.AddAuditAsync(new Audit
            {
                EventName = eventName,
                AuditType = AuditType.Subscribe,
                WebhookUri = webhookUri
            });
        }

        public async Task UnsubscribeAsync(string eventName, Uri webhookUri)
        {
            var subscription = _webhooksRepository.Subscriptions.FirstOrDefault(s => s.EventName == eventName && s.WebhookUri == webhookUri);
            if (subscription is null)
                return;

            await _webhooksRepository.RemoveSubscriptionAsync(subscription);
            await _webhooksRepository.AddAuditAsync(new Audit
            {
                EventName = eventName,
                AuditType = AuditType.Unsubscribe,
                WebhookUri = webhookUri
            });
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent
        {
            await _webhooksRepository.AddAuditAsync(new Audit
            {
                EventName = @event.GetType().FullName!,
                AuditType = AuditType.Publish,
                Payload = JsonConvert.SerializeObject(@event)
            });

            foreach (var subscription in _webhooksRepository.Subscriptions.Where(s => s.EventName == @event.GetType().FullName && s.IsActive).ToList())
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
                await _webhooksRepository.AddAuditAsync(audit);
            }
            catch (Exception ex)
            {
                audit.Error = ex.Message.Length > 1024 ? ex.Message[..1024] : ex.Message;
                await _webhooksRepository.AddAuditAsync(audit);
                throw;
            }
        }

        public async Task<WebhooksHealthSummary> GetHealthSummaryAsync(TimeSpan period)
        {
            var startDateUtc = DateTime.UtcNow - period;
            var audits = await _webhooksRepository.Audits
                .Where(x => x.CreatedUtc >= startDateUtc)
                .GroupBy(x => new { x.EventName, x.WebhookUri, x.AuditType, x.Error })
                .Select(x => new WebhooksHealthSummary.AuditSummary
                {
                    EventName = x.Key.EventName,
                    WebhookUri = x.Key.WebhookUri,
                    AuditType = x.Key.AuditType,
                    Error = x.Key.Error,
                    Count = x.Count()
                })
                .ToListAsync();
            var sortedAudits = audits
                .OrderBy(x => x.EventName)
                .ThenBy(x => x.WebhookUri)
                .ThenBy(x => x.AuditType)
                .Take(100);

            return new WebhooksHealthSummary
            {
                PublishIntervalMinutes = (int)period.TotalMinutes,
                Audits = [.. audits]
            };
        }

        protected virtual Task OnPublishAsync<TEvent>(Subscription subscription, TEvent @event) where TEvent : WebhookEvent => DispatchAsync(subscription.WebhookUri, @event);

        #endregion
    }
}
