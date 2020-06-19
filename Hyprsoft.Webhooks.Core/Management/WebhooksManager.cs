using Hyprsoft.Webhooks.Core.Events;
using Hyprsoft.Webhooks.Core.Rest;
using Serialize.Linq.Nodes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Management
{
    public interface IWebhooksManager : IDisposable
    {
        Task SubscribeAsync(string typeName, Uri webhookUri, ExpressionNode filter = null);

        Task UnsubscribeAsync(string typeName, Uri webhookUri);

        Task PublishAsync<TEvent>(TEvent @event) where TEvent : WebhookEvent;

        Task<IEnumerable<Subscription>> GetSubscriptionsAsync();
    }

    public abstract class WebhooksManager : IDisposable
    {
        #region Fields

        private bool _isDisposed;

        #endregion

        #region Constructors

        public WebhooksManager(WebhooksHttpClientOptions options)
        {
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

        protected WebhooksHttpClient HttpClient { get; }

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
