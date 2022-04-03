using Hyprsoft.Webhooks.Core.Rest;
using Microsoft.Extensions.Options;

namespace Hyprsoft.Webhooks.Core.Management
{
    internal sealed class InMemoryWebhooksManager : WebhooksManager, IWebhooksManager
    {
        #region Constructors

        public InMemoryWebhooksManager(IWebhooksStorageProvider storageProvider, IOptions<WebhooksHttpClientOptions> options) : base(storageProvider, options) { }

        #endregion
    }
}
