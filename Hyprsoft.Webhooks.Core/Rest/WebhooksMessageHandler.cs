using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Rest
{
    internal class WebhooksMessageHandler : DelegatingHandler
    {
        #region Fields

        private readonly string _payloadSigningSecret;

        #endregion

        #region Constructors

        public WebhooksMessageHandler(string payloadSigningSecret) : base(new HttpClientHandler())
        {
            _payloadSigningSecret = payloadSigningSecret;
        }

        #endregion

        #region Methods

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(_payloadSigningSecret))
                throw new InvalidOperationException("Missing client secret.  Please check your configuration.");

            request.Headers.Remove(WebhooksHttpClient.PayloadSignatureHeaderName);
            var requestPayload = await request.Content.ReadAsStringAsync();
            var signature = WebhooksHttpClient.GetSignature(_payloadSigningSecret, requestPayload);
            request.Headers.Add(WebhooksHttpClient.PayloadSignatureHeaderName, signature);

            return await base.SendAsync(request, cancellationToken);
        }

        #endregion
    }
}
