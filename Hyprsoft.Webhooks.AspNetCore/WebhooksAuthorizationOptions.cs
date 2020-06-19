using Microsoft.AspNetCore.Http;
using Hyprsoft.Webhooks.Core;

namespace Hyprsoft.Webhooks.AspNetCore
{
    public sealed class WebhooksAuthorizationOptions
    {
        public string PayloadSigningSecret { get; set; } = WebhooksGlobalConfiguration.DefaultPayloadSigningSecret;

        public PathString Segments { get; set; } = "/webhooks";
    }
}
