using Hyprsoft.Webhooks.Core.Rest;

namespace Hyprsoft.Webhooks.Core.Management
{
    internal sealed class InMemoryWebhooksManager : WebhooksManager, IWebhooksManager
    {
        #region Constructors

        public InMemoryWebhooksManager(IWebhooksStorageProvider storageProvider, WebhooksHttpClientOptions options) : base(storageProvider, options) { }

        #endregion
    }
}
