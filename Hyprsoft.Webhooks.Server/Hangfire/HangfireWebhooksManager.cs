using Hangfire;
using Hyprsoft.Webhooks.Core;

namespace Hyprsoft.Webhooks.Server
{
    internal sealed class HangfireWebhooksManager : WebhooksManager
    {
        #region Constructors

        public HangfireWebhooksManager(IWebhooksRepository database, HttpClient httpClient) : base(database, httpClient) { }

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
