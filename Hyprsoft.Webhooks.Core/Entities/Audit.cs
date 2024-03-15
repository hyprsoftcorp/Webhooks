using System.ComponentModel.DataAnnotations;

namespace Hyprsoft.Webhooks.Core
{
    public class Audit
    {
        [Key, Required]
        public int AuditId { get; set; }

        [Required, MaxLength(100)]
        public string EventName { get; set; } = null!;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public Uri? WebhookUri { get; set; }

        public string? Payload { get; set; }

        [MaxLength(1024)]
        public string? Error { get; set; }

        public AuditType AuditType { get; set; }
    }
}
