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

        [Required, MaxLength(255)]
        public Uri WebhookUri { get; set; } = null!;

        public string Payload { get; set; } = null!;

        [MaxLength(1024)]
        public string? Error { get; set; }
    }
}
