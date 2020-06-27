using Hyprsoft.Webhooks.Core.Management;
using System.Linq;
using System.Threading.Tasks;

namespace Hyprsoft.Webhooks.Core.Hangfire
{
    public class HangfireWebhooksStorageProvider : IWebhooksStorageProvider
    {
        #region Fields

        private readonly WebhooksDbContext _database;

        #endregion

        #region Constructors

        public HangfireWebhooksStorageProvider(WebhooksDbContext database)
        {
            _database = database;
        }

        #endregion

        #region Methods

        public IQueryable<Subscription> Subscriptions => _database.Subscriptions;

        public async Task AddAuditAsync(Audit audit)
        {
            await _database.Audits.AddAsync(audit);
            await _database.SaveChangesAsync();
        }

        public async Task RemoveSubscriptionAsync(Subscription subscription)
        {
            _database.Subscriptions.Remove(subscription);
            await _database.SaveChangesAsync();
        }

        public async Task UpsertSubscriptionAsync(Subscription subscription)
        {
            if (subscription.SubscriptionId <= 0)
                await _database.Subscriptions.AddAsync(subscription);
            else
                _database.Subscriptions.Update(subscription);
            await _database.SaveChangesAsync();
        }

        #endregion
    }
}
