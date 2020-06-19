using System;
using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Webhooks.Core.Management
{
    public class Subscription
    {
        [Key, Required]
        public int SubscriptionId { get; set; }

        [Required]
        public string TypeName { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [Required]
        public Uri WebhookUri { get; set; }

        public string FilterExpression { get; set; }
        
        public string Filter { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
