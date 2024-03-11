using Hangfire;
using Hyprsoft.Webhooks.Client;
using Hyprsoft.Webhooks.Core;
using Microsoft.Extensions.Options;

namespace Hyprsoft.Webhooks.Server
{
    internal class HangfireWebhooksManager : WebhooksManager
    {
        #region Constructors

        public HangfireWebhooksManager(IWebhooksRepository database, IOptions<WebhooksHttpClientOptions> options) : base(database, options) { }

        #endregion

        #region Methods

        protected override Task OnPublishAsync<TEvent>(Subscription subscription, TEvent @event)
        {
            // Hangfire gives us scalable queueing and automatic retry logic with exponential backoff, etc.
            BackgroundJob.Enqueue<IWebhooksManager>(x => x.DispatchAsync(subscription.WebhookUri, @event));

            return Task.CompletedTask;
        }

        #endregion
    }
}
