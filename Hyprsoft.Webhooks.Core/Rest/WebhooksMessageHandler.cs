using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Rest
{
    internal class WebhooksMessageHandler : DelegatingHandler
    {
        #region Fields

        private readonly string _apiKey;

        #endregion

        #region Constructors

        public WebhooksMessageHandler(string apiKey) : base(new HttpClientHandler()) => _apiKey = apiKey;

        #endregion

        #region Methods

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Missing api key.  Please check your configuration.");

            request.Headers.Remove(WebhooksHttpClient.ApiKeyHeaderName);
            request.Headers.Add(WebhooksHttpClient.ApiKeyHeaderName, _apiKey);

            return await base.SendAsync(request, cancellationToken);
        }

        #endregion
    }
}
