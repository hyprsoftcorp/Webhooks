using Newtonsoft.Json;

namespace Hyprsoft.Webhooks.Core
{
    public static class WebhooksGlobalConfiguration
    {
        public const string LatestWebhooksApiVersion = "1.0";
        public const string DefaultPayloadSigningSecret = "A68B5DB1237B410BB1447E1130DAEF1B0752988081784DBD945A661A416E1AD5";

        public static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
    }
}
