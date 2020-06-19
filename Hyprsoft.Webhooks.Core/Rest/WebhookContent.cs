using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace Hyprsoft.Webhooks.Core.Rest
{
    public class WebhookContent : StringContent
    {
        public WebhookContent(object content) : base(JsonConvert.SerializeObject(content, WebhooksGlobalConfiguration.JsonSerializerSettings), Encoding.UTF8, "application/json") { }
    }
}
