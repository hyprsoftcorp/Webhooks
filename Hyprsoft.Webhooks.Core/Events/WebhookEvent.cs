using System;

namespace Hyprsoft.Webhooks.Core.Events
{
    public abstract class WebhookEvent
    {
        public Guid WebhookId { get; set; } = Guid.NewGuid();

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
