using Hyprsoft.Webhooks.Core;
using Newtonsoft.Json;
using System.Text;

namespace Hyprsoft.Webhooks.Client
{
    internal class WebhookContent : StringContent
    {
        public WebhookContent(object content) : base(JsonConvert.SerializeObject(content, WebhooksGlobalConfiguration.JsonSerializerSettings), Encoding.UTF8, "application/json") { }
    }
}
