using Hyprsoft.Webhooks.Core;

namespace Hyprsoft.Webhooks.Server
{
    internal interface IWebhooksRepository
    {
        IQueryable<Audit> Audits { get; }

        IQueryable<Subscription> Subscriptions { get; }

        Task AddAuditAsync(Audit audit);

        Task UpsertSubscriptionAsync(Subscription subscription);

        Task RemoveSubscriptionAsync(Subscription subscription);
    }

    internal sealed class WebhooksRepository : IWebhooksRepository
    {
        #region Fields

        private readonly WebhooksDbContext _database;

        #endregion

        #region Constructors

        public WebhooksRepository(WebhooksDbContext database) => _database = database;

        #endregion

        #region Properties

        public IQueryable<Audit> Audits => _database.Audits;

        public IQueryable<Subscription> Subscriptions => _database.Subscriptions;

        #endregion

        #region Methods

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
