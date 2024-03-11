using Hyprsoft.Webhooks.Core;
using Newtonsoft.Json;

namespace Hyprsoft.Webhooks.Client
{
    internal class WebhooksHttpClient : HttpClient
    {
        internal WebhooksHttpClient(string apiKey) : base(new WebhooksMessageHandler(apiKey)) { }

        public const string ApiKeyHeaderName = "X-API-KEY";

#pragma warning disable CA1822 // Mark members as static
        public async Task ValidateResponseAsync(HttpResponseMessage message, string error)
#pragma warning restore CA1822 // Mark members as static
        {
            var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
            WebhookResponse? responsePayload = null;
            try
            {
                // responsePayload can be null if content is an empty string.
                responsePayload = JsonConvert.DeserializeObject<WebhookResponse>(content);
            }
            catch (Exception)
            {
            }
            responsePayload ??= new WebhookResponse
                {
                    IsSuccess = false,
                    ErrorMessage = String.IsNullOrWhiteSpace(content) ? "Unknown." : content
                };

            if (message.IsSuccessStatusCode || responsePayload.IsSuccess)
                return;

            throw new WebhookException($"{error} | Status: {(int)message.StatusCode} {message.ReasonPhrase} | Reason: {responsePayload.ErrorMessage ?? content}");
        }
    }
}
