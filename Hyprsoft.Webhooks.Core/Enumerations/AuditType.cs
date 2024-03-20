using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hyprsoft.Webhooks.Core
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuditType
    {
        Dispatch,
        Publish,
        Subscribe,
        Unsubscribe
    }
}
