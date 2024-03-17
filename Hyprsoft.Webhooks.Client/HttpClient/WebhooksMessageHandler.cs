namespace Hyprsoft.Webhooks.Client
{
    internal sealed class WebhooksMessageHandler : DelegatingHandler
    {
        #region Fields

        public const string ApiKeyHeaderName = "X-API-KEY";
        private readonly string _apiKey;

        #endregion

        #region Constructors

        public WebhooksMessageHandler(string apiKey) => _apiKey = apiKey;

        #endregion

        #region Methods

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Missing api key.  Please check your configuration.");

            request.Headers.Remove(ApiKeyHeaderName);
            request.Headers.Add(ApiKeyHeaderName, _apiKey);

            return await base.SendAsync(request, cancellationToken);
        }

        #endregion
    }
}
