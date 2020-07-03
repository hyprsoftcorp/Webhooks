﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Management
{
    public class InMemoryWebhooksStorageProvider : IWebhooksStorageProvider
    {
        #region Fields

        private readonly Dictionary<int, Audit> _audits = new Dictionary<int, Audit>();
        private readonly Dictionary<int, Subscription> _subscriptions = new Dictionary<int, Subscription>();

        #endregion

        #region Properties

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
            if (_subscriptions.ContainsKey(subscription.SubscriptionId))
                _subscriptions.Remove(subscription.SubscriptionId);
            
            return Task.CompletedTask;
        }
        
        #endregion
    }
}
