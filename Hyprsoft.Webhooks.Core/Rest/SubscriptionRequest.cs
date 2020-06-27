using Serialize.Linq.Nodes;
using System;

namespace Hyprsoft.Webhooks.Core.Rest
{
    public class SubscriptionRequest
    {
        public string EventName { get; set; }

        public Uri WebhookUri { get; set; }

        public ExpressionNode Filter { get; set; }
    }
}