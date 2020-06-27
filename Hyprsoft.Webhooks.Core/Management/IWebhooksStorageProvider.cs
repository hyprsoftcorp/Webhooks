using System.Linq;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Management
{
    public interface IWebhooksStorageProvider
    {
        IQueryable<Subscription> Subscriptions { get; }

        Task AddAuditAsync(Audit audit);

        Task UpsertSubscriptionAsync(Subscription subscription);

        Task RemoveSubscriptionAsync(Subscription subscription);
    }
}
