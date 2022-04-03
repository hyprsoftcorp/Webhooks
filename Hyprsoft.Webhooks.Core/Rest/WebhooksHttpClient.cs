using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Rest
{
    public class WebhooksHttpClient : HttpClient
    {
        internal WebhooksHttpClient(string apiKey) : base(new WebhooksMessageHandler(apiKey)) { }

        public const string ApiKeyHeaderName = "X-API-KEY";

        public async Task ValidateResponseAsync(HttpResponseMessage message, string error)
        {
            var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
            WebhookResponse responsePayload = null;
            try
            {
                // responsePayload can be null if content is an empty string.
                responsePayload = JsonConvert.DeserializeObject<WebhookResponse>(content);
            }
            catch (Exception)
            {
            }
            if (responsePayload == null)
            {
                responsePayload = new WebhookResponse
                {
                    IsSuccess = false,
                    ErrorMessage = String.IsNullOrWhiteSpace(content) ? "Unknown." : content
                };
            }

            if (message.IsSuccessStatusCode || responsePayload.IsSuccess)
                return;

            throw new WebhookException($"{error} | Status: {(int)message.StatusCode} {message.ReasonPhrase} | Reason: {responsePayload.ErrorMessage ?? content}");
        }
    }
}
