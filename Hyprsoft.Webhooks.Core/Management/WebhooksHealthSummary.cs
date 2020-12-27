using System;
using System.Collections.Generic;

namespace Hyprsoft.Webhooks.Core.Management
{
    public class WebhooksHealthSummary
    {
        public class SuccessfulWebhook
        {
            public string EventName { get; set; }
            public int Count { get; set; }
        }

        public class FailedWebook : SuccessfulWebhook
        {
            public Uri WebhookUri { get; set; }
            public string Error { get; set; }
        }

        private DateTime _serverStartDateTimeUtc;
        private string _uptime;

        public DateTime ServerStartDateUtc
        {
            get => _serverStartDateTimeUtc;
            set
            {
                _serverStartDateTimeUtc = value;
                var duration = DateTime.UtcNow - ServerStartDateUtc;
                _uptime = $"{duration.Days} {(duration.Days == 1 ? "day" : "days")} {duration.Hours} {(duration.Hours == 1 ? "hour" : "hours")} {duration.Minutes} {(duration.Minutes == 1 ? "minute" : "minutes")} and {duration.Seconds} {(duration.Seconds == 1 ? "second" : "seconds")}";
            }
        }

        public int PublishIntervalMinutes { get; set; }

        public string Uptime { get => _uptime; set => _uptime = value; }

        public IReadOnlyList<SuccessfulWebhook> SuccessfulWebhooks { get; set; }

        public IReadOnlyList<FailedWebook> FailedWebhooks { get; set; }
    }
}
