using Newtonsoft.Json;
using System;
using System.Net;

namespace Hyprsoft.Webhooks.Core.Events
{
    public abstract class WebhookEvent
    {
        private static readonly string _hostname;

        static WebhookEvent()
        {
            _hostname = Dns.GetHostName().ToUpper();
        }

        [JsonProperty]
        public Guid WebhookId { get; internal set; } = Guid.NewGuid();

        [JsonProperty]
        public DateTime CreatedUtc { get; internal set; } = DateTime.UtcNow;

        [JsonProperty]
        public string OriginatorHostname { get; internal set; } = _hostname;

    }
}
