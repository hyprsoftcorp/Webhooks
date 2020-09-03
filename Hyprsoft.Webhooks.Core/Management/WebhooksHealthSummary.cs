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
        
        public DateTime ServerStartDateUtc { get; set; }
      
        public TimeSpan PublishInterval { get; set; }

        public IReadOnlyList<SuccessfulWebhook> SuccessfulWebhooks { get; set; }

        public IReadOnlyList<FailedWebook> FailedWebhooks { get; set; } 
    }
}
