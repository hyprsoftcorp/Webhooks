using Hangfire;
using Microsoft.EntityFrameworkCore;
using Hyprsoft.Webhooks.Core.Events;
using Hyprsoft.Webhooks.Core.Management;
using Hyprsoft.Webhooks.Core.Rest;
using Newtonsoft.Json;
using Serialize.Linq.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Hangfire
{
    public interface IHangfireWebhooksManager : IWebhooksManager
    {
        Task DispatchAsync<TEvent>(Uri webhookUri, TEvent @event) where TEvent : WebhookEvent;
    }

    public class HangfireWebhooksManager : WebhooksManager, IHangfireWebhooksManager
    {
        #region Fields

        private readonly WebhooksDbContext _database;

        #endregion

        #region Constructors

        public HangfireWebhooksManager(WebhooksDbContext database, WebhooksHttpClientOptions options) : base(options)
        {
            _database = database;
        }

        #endregion

        #region Methods

        public async Task SubscribeAsync(string typeName, Uri webhookUri, ExpressionNode filter = null)
        {
            var node = filter == null ? null : JsonConvert.SerializeObject(filter, WebhooksGlobalConfiguration.JsonSerializerSettings);
            var subscription = _database.Subscriptions.FirstOrDefault(s => s.TypeName == typeName && s.WebhookUri == webhookUri);
            if (subscription == null)
            {
                _database.Subscriptions.Add(new Subscription
                {
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
            await _database.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UnsubscribeAsync(string typeName, Uri webhookUri)
        {
            var @event = _database.Subscriptions.FirstOrDefault(s => s.TypeName == typeName && s.WebhookUri == webhookUri);
            if (@event != null)
            {
                _database.Subscriptions.Remove(@event);
                await _database.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent
        {
            foreach (var subscription in _database.Subscriptions.Where(s => s.TypeName == @event.GetType().FullName && s.IsActive).AsNoTracking())
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
                    // Hangfire gives us scalable queueing and automatic retry logic with exponential backoff, etc.
                    BackgroundJob.Enqueue<IHangfireWebhooksManager>(x => x.DispatchAsync(subscription.WebhookUri, @event));
                }
            }
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsAsync() => await _database.Subscriptions.OrderBy(x => x.CreatedUtc).AsNoTracking().ToListAsync();

        public async Task DispatchAsync<TEvent>(Uri webhookUri, TEvent @event) where TEvent : WebhookEvent
        {
            var response = await HttpClient.PostAsync(webhookUri, new WebhookContent(@event)).ConfigureAwait(false);
            await HttpClient.ValidateResponseAsync(response, "Dispatch failed.");
        }

        #endregion
    }
}
