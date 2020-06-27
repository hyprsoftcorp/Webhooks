using Hangfire;
using Hyprsoft.Webhooks.Core.Events;
using Hyprsoft.Webhooks.Core.Management;
using Hyprsoft.Webhooks.Core.Rest;
using System;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Hangfire
{
    public interface IHangfireWebhooksManager : IWebhooksManager
    {
        Task DispatchAsync<TEvent>(Uri webhookUri, TEvent @event) where TEvent : WebhookEvent;
    }

    public class HangfireWebhooksManager : WebhooksManager, IHangfireWebhooksManager
    {
        #region Constructors

        public HangfireWebhooksManager(IWebhooksStorageProvider storageProvider, WebhooksHttpClientOptions options) : base(storageProvider, options) { }

        #endregion

        #region Methods

        protected override Task OnPublishAsync<TEvent>(Subscription subscription, TEvent @event)
        {
            // Hangfire gives us scalable queueing and automatic retry logic with exponential backoff, etc.
            BackgroundJob.Enqueue<IHangfireWebhooksManager>(x => x.DispatchAsync(subscription.WebhookUri, @event));

            return Task.CompletedTask;
        }

        #endregion
    }
}
