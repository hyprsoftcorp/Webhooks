using Hyprsoft.Webhooks.Core;
using Newtonsoft.Json;

namespace Hyprsoft.Webhooks.Client
{
    internal static class HttpClientExtensions
    {
        public static async Task ValidateResponseAsync(this HttpClient _, HttpResponseMessage message, string error)
        {
            var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
            WebhookResponse? responsePayload = null;
            try
            {
                // responsePayload can be null if content is an empty string.
                responsePayload = JsonConvert.DeserializeObject<WebhookResponse>(content);
            }
            catch (Exception) { }

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
