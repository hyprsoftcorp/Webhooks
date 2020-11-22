using Newtonsoft.Json;
using System;

namespace Hyprsoft.Webhooks.Core.Events
{
    public abstract class WebhookEvent
    {
        [JsonProperty]
        public Guid WebhookId { get; internal set; } = Guid.NewGuid();

        [JsonProperty]
        public DateTime CreatedUtc { get; internal set; } = DateTime.UtcNow;
    }
}
