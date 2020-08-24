using System.Linq;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Management
{
    public interface IWebhooksStorageProvider
    {
        IQueryable<Audit> Audits { get; }
        
        IQueryable<Subscription> Subscriptions { get; }

        Task AddAuditAsync(Audit audit);

        Task UpsertSubscriptionAsync(Subscription subscription);

        Task RemoveSubscriptionAsync(Subscription subscription);
    }
}
