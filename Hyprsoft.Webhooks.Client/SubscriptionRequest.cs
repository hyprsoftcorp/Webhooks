using Serialize.Linq.Nodes;

namespace Hyprsoft.Webhooks.Client
{
    public sealed record SubscriptionRequest(string EventName, Uri WebhookUri, ExpressionNode? Filter);
}