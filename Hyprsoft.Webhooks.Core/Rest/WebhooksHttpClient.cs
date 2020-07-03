using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Rest
{
    public class WebhooksHttpClient : HttpClient
    {
        public WebhooksHttpClient(string payloadSigningSecret) : base(new WebhooksMessageHandler(payloadSigningSecret)) { }

        public const string PayloadSignatureHeaderName = "Webhooks-Payload-Signature";

        public static string GetSignature(string payloadSigningSecret, string payload)
        {
            using (var hasher = new HMACSHA256(Encoding.UTF8.GetBytes(payloadSigningSecret)))
            {
                return Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(payload)));
            }
        }

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

            throw new WebhookException($"{error} | Status: {(int)message.StatusCode} {message.ReasonPhrase} | Reason: {responsePayload.ErrorMessage}");
        }
    }
}
