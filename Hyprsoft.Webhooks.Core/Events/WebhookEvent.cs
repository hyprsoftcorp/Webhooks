using System;

namespace Hyprsoft.Webhooks.Core.Events
{
    public abstract class WebhookEvent
    {
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
