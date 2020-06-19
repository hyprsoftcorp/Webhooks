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
    public sealed class InMemoryWebhooksManager : WebhooksManager, IWebhooksManager
    {
        #region Fields

        private readonly List<Subscription> _subscriptions = new List<Subscription>();

        #endregion

        #region Constructors

        public InMemoryWebhooksManager(WebhooksHttpClientOptions options) : base(options) { }

        #endregion

        #region Methods

        public Task SubscribeAsync(string typeName, Uri webhookUri, ExpressionNode filter = null)
        {
            var node = filter == null ? null : JsonConvert.SerializeObject(filter, WebhooksGlobalConfiguration.JsonSerializerSettings);
            var subscription = _subscriptions.FirstOrDefault(s => s.TypeName == typeName && s.WebhookUri == webhookUri);
            if (subscription == null)
            {
                _subscriptions.Add(new Subscription
                {
                    SubscriptionId = _subscriptions.Count > 0 ? _subscriptions.Max(x => x.SubscriptionId) + 1 : 1,
                    TypeName = typeName,
                    WebhookUri = webhookUri,
                    FilterExpression = node,
                    Filter = filter?.ToString()
                });
            }
            else
            {
                subscription.IsActive = true;
                subscription.CreatedUtc = DateTime.UtcNow;
                subscription.FilterExpression = node;
                subscription.Filter = filter?.ToString();
            }

            return Task.CompletedTask;
        }

        public Task UnsubscribeAsync(string typeName, Uri webhookUri)
        {
            var @event = _subscriptions.FirstOrDefault(s => s.TypeName == typeName && s.WebhookUri == webhookUri);
            if (@event != null)
                _subscriptions.Remove(@event);

            return Task.CompletedTask;
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent
        {
            foreach (var subscription in _subscriptions.Where(s => s.TypeName == @event.GetType().FullName && s.IsActive))
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
                    var response = await HttpClient.PostAsync(subscription.WebhookUri, new WebhookContent(@event)).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                        throw new WebhookException($"Unable to dispatch event '{@event.GetType().FullName}' with payload '{JsonConvert.SerializeObject(@event, WebhooksGlobalConfiguration.JsonSerializerSettings)}' to '{subscription.WebhookUri}'.  Reason: {await response.Content.ReadAsStringAsync().ConfigureAwait(false)}");
                }
            }
        }

        public Task<IEnumerable<Subscription>> GetSubscriptionsAsync() => Task.FromResult(_subscriptions.OrderBy(x => x.CreatedUtc).AsEnumerable());

        #endregion
    }
}
