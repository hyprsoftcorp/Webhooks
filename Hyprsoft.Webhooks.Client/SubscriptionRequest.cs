using Serialize.Linq.Nodes;

namespace Hyprsoft.Webhooks.Client
{
    public class SubscriptionRequest
    {
        public string EventName { get; set; } = null!;

        public Uri WebhookUri { get; set; } = null!;

        public ExpressionNode? Filter { get; set; }
    }
}