using Newtonsoft.Json;
using System.Net;

namespace Hyprsoft.Webhooks.Events
{
    public abstract class WebhookEvent
    {
        private static readonly string _hostname;

        static WebhookEvent() => _hostname = $"{Dns.GetHostName().ToLower()}.{AppDomain.CurrentDomain.FriendlyName.ToLower()}";

        [JsonProperty]
        public Guid WebhookId { get; internal set; } = Guid.NewGuid();

        [JsonProperty]
        public DateTime CreatedUtc { get; internal set; } = DateTime.UtcNow;

        [JsonProperty]
        public string Originator { get; internal set; } = _hostname;

    }
}
