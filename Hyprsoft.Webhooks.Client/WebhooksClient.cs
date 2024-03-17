using Hyprsoft.Webhooks.Core;
using Hyprsoft.Webhooks.Events;
using Serialize.Linq.Extensions;
using System.Linq.Expressions;

namespace Hyprsoft.Webhooks.Client
{
    public interface IWebhooksClient
    {
        Task SubscribeAsync<TEvent>(Uri webhookUri, Expression<Func<TEvent, bool>>? filterExpression = null) where TEvent : WebhookEvent;

        Task UnsubscribeAsync<TEvent>(Uri webhookUri) where TEvent : WebhookEvent;

        Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent;
    }

    public sealed class WebhooksClient : IWebhooksClient
    {
        #region Fields

        private readonly HttpClient _httpClient;

        #endregion

        #region Constructors

        public WebhooksClient(HttpClient httpClient) => _httpClient = httpClient;

        #endregion

        #region Methods

        public async Task SubscribeAsync<TEvent>(Uri webhookUri, Expression<Func<TEvent, bool>>? filterExpression = null) where TEvent : WebhookEvent
        {
            var requestPayload = new SubscriptionRequest(typeof(TEvent).FullName!, webhookUri, filterExpression?.ToExpressionNode());
            var response = await _httpClient.PutAsync($"webhooks/v{WebhooksGlobalConfiguration.LatestWebhooksApiVersion}/subscribe", new WebhookContent(requestPayload)).ConfigureAwait(false);
            await _httpClient.ValidateResponseAsync(response, "Subscribe failed.");
        }

        public async Task UnsubscribeAsync<TEvent>(Uri webhookUri) where TEvent : WebhookEvent
        {
            var requestPayload = new SubscriptionRequest(typeof(TEvent).FullName!, webhookUri, null);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"webhooks/v{WebhooksGlobalConfiguration.LatestWebhooksApiVersion}/unsubscribe"),
                Content = new WebhookContent(requestPayload)
            };
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            await _httpClient.ValidateResponseAsync(response, "Unsubscribe failed.");
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent
        {
            var response = await _httpClient.PostAsync($"webhooks/v{WebhooksGlobalConfiguration.LatestWebhooksApiVersion}/publish", new WebhookContent(@event)).ConfigureAwait(false);
            await _httpClient.ValidateResponseAsync(response, "Publish failed.");
        }

        #endregion
    }
}
