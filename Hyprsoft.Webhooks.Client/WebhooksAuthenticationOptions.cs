using Hyprsoft.Webhooks.Core;

namespace Hyprsoft.Webhooks.Client
{
    public sealed class WebhooksAuthenticationOptions
    {
        public string ApiKey { get; set; } = WebhooksGlobalConfiguration.DefaultApiKey;
        public string ApiUserIdentifier { get; set; } = WebhooksGlobalConfiguration.DefaultApiUserIdentifier;
        public string ApiUserUserName { get; set; } = WebhooksGlobalConfiguration.DefaultApiUsername;

    }
}
