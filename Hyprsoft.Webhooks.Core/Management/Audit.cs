using System;
using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Webhooks.Core.Management
{
    public class Audit
    {
        [Key, Required]
        public int AuditId { get; set; }

        [Required, MaxLength(100)]
        public string EventName { get; set; }

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(1024)]
        public Uri WebhookUri { get; set; }

        [MaxLength(255)]
        public string Filter { get; set; }

        public string Payload { get; set; }

        public string Error { get; set; }
    }
}
