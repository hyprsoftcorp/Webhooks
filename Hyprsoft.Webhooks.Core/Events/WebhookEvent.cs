using System;

namespace Hyprsoft.Webhooks.Core.Events
{
    public abstract class WebhookEvent
    {
        public Guid WebhookId { get; internal set; } = Guid.NewGuid();

        public DateTime CreatedUtc { get; internal set; } = DateTime.UtcNow;
    }
}
