using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Management
{
    internal class InMemoryWebhooksStorageProvider : IWebhooksStorageProvider
    {
        #region Fields

        private readonly Dictionary<int, Audit> _audits = [];
        private readonly Dictionary<int, Subscription> _subscriptions = [];

        #endregion

        #region Properties

        public IQueryable<Audit> Audits => _audits.Values.AsQueryable();
        
        public IQueryable<Subscription> Subscriptions => _subscriptions.Values.AsQueryable();

        #endregion

        #region Methods

        public Task AddAuditAsync(Audit audit)
        {
            if (audit.AuditId <= 0)
                audit.AuditId = _audits.Count > 0 ? _audits.Values.Max(x => x.AuditId) + 1 : 1;
            _audits[audit.AuditId] = audit;

            return Task.CompletedTask;
        }

        public Task UpsertSubscriptionAsync(Subscription subscription)
        {
            if (subscription.SubscriptionId <= 0)
                subscription.SubscriptionId = _subscriptions.Count > 0 ? _subscriptions.Values.Max(x => x.SubscriptionId) + 1 : 1;
            _subscriptions[subscription.SubscriptionId] = subscription;
          
            return Task.CompletedTask;
        }

        public Task RemoveSubscriptionAsync(Subscription subscription)
        {
            _subscriptions.Remove(subscription.SubscriptionId);

            return Task.CompletedTask;
        }
        
        #endregion
    }
}
