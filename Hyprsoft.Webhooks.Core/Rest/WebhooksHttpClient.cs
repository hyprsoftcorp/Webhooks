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
            var responsePayload = JsonConvert.DeserializeObject<WebhookResponse>(await message.Content.ReadAsStringAsync().ConfigureAwait(false));

            if (message.IsSuccessStatusCode || (responsePayload != null && responsePayload.IsSuccess))
                return;

            var reason = "n/a";
            if (!String.IsNullOrWhiteSpace(responsePayload?.ErrorMessage))
                reason = responsePayload.ErrorMessage;
            else
            {
                var content = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!String.IsNullOrWhiteSpace(content))
                    reason = content;
            }
            throw new WebhookException($"{error} | Status: {(int)message.StatusCode} {message.ReasonPhrase} | Reason: {reason}");
        }
    }
}
