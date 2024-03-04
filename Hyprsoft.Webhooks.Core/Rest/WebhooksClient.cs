using Hyprsoft.Webhooks.Core.Events;
using Microsoft.Extensions.Options;
using Serialize.Linq.Extensions;
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Rest
{
    public interface IWebhooksClient : IDisposable
    {
        Task SubscribeAsync<TEvent>(Uri webhookUri, Expression<Func<TEvent, bool>> filterExpression = null) where TEvent : WebhookEvent;

        Task UnsubscribeAsync<TEvent>(Uri webhookUri) where TEvent : WebhookEvent;

        Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent;
    }

    public class WebhooksClient : IWebhooksClient, IDisposable
    {
        #region Fields

        private bool _isDisposed;
        private readonly WebhooksHttpClient _client;

        #endregion

        #region Constructors

        public WebhooksClient(IOptions<WebhooksHttpClientOptions> options)
        {
            Options = options.Value;
            _client = new WebhooksHttpClient(Options.ApiKey)
            {
                BaseAddress = Options.ServerBaseUri,
                Timeout = Options.RequestTimeout
            };
        }

        #endregion

        #region Properties

        public WebhooksHttpClientOptions Options { get; }

        #endregion

        #region Methods

        public async Task SubscribeAsync<TEvent>(Uri webhookUri, Expression<Func<TEvent, bool>> filterExpression = null) where TEvent : WebhookEvent
        {
            var requestPayload = new SubscriptionRequest
            {
                EventName = typeof(TEvent).FullName,
                WebhookUri = webhookUri,
                Filter = filterExpression.ToExpressionNode()
            };
            var response = await _client.PutAsync($"webhooks/v{WebhooksGlobalConfiguration.LatestWebhooksApiVersion}/subscribe", new WebhookContent(requestPayload)).ConfigureAwait(false);
            await _client.ValidateResponseAsync(response, "Subscribe failed.");
        }

        public async Task UnsubscribeAsync<TEvent>(Uri webhookUri) where TEvent : WebhookEvent
        {
            var requestPayload = new SubscriptionRequest
            {
                EventName = typeof(TEvent).FullName,
                WebhookUri = webhookUri
            };
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{Options.ServerBaseUri}webhooks/v{WebhooksGlobalConfiguration.LatestWebhooksApiVersion}/unsubscribe"),
                Content = new WebhookContent(requestPayload)
            };
            var response = await _client.SendAsync(request).ConfigureAwait(false);
            await _client.ValidateResponseAsync(response, "Unsubscribe failed.");
        }

        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent
        {
            var response = await _client.PostAsync($"webhooks/v{WebhooksGlobalConfiguration.LatestWebhooksApiVersion}/publish", new WebhookContent(@event)).ConfigureAwait(false);
            await _client.ValidateResponseAsync(response, "Publish failed.");
        }

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                _client?.Dispose();
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
