using Hyprsoft.Webhooks.Core;

namespace Hyprsoft.Webhooks.Server
{
    public sealed class WebhooksHealthSummary
    {
        public record AuditSummary
        {
            public string EventName { get; set; } = null!;
            public Uri? WebhookUri { get; set; }
            public AuditType AuditType { get; set; }
            public string? Error { get; set; }
            public int Count { get; set; }
        }

        private DateTime _serverStartDateTimeUtc;
        private string _uptime = GetUptimeMessage(TimeSpan.Zero);

        public DateTime ServerStartDateUtc
        {
            get => _serverStartDateTimeUtc;
            set
            {
                _serverStartDateTimeUtc = value;
                _uptime = GetUptimeMessage(DateTime.UtcNow - ServerStartDateUtc);
            }
        }

        public int PublishIntervalMinutes { get; set; }

        public string Uptime 
        { 
            get => _uptime; 
            set => _uptime = value; 
        }

        public IReadOnlyList<AuditSummary> Audits { get; set; } = [];

        private static string GetUptimeMessage(TimeSpan duration) => $"{duration.Days} {(duration.Days == 1 ? "day" : "days")} {duration.Hours} {(duration.Hours == 1 ? "hour" : "hours")} {duration.Minutes} {(duration.Minutes == 1 ? "minute" : "minutes")} and {duration.Seconds} {(duration.Seconds == 1 ? "second" : "seconds")}";
    }
}
