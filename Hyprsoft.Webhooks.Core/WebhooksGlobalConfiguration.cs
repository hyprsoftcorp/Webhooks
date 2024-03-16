using Newtonsoft.Json;

namespace Hyprsoft.Webhooks.Core
{
    public static class WebhooksGlobalConfiguration
    {
        public const string LatestWebhooksApiVersion = "1.0";
        public const string DefaultApiKey = "kA8rR4lKXaQT9BsbFCKrsqziB";
        public const string DefaultApiUserIdentifier = "448A0C99-6F82-4EAF-A1F9-3F131094F05B";
        public const string DefaultApiUsername = "Webhooks API User";
        public const string WebhooksDbName = "WebhooksDb";

        public static readonly JsonSerializerSettings JsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };
    }
}
