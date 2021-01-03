using System;
using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Webhooks.Core.Management
{
    public class Subscription
    {
        [Key, Required]
        public int SubscriptionId { get; set; }

        [Required, MaxLength(100)]
        public string EventName { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(255)]
        public Uri WebhookUri { get; set; }

        public string FilterExpression { get; set; }
        
        [MaxLength(255)]
        public string Filter { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
