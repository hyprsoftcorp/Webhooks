using Hyprsoft.Webhooks.Core;
using Microsoft.EntityFrameworkCore;

namespace Hyprsoft.Webhooks.Server
{
    internal class WebhooksDbContext : DbContext
    {
        public WebhooksDbContext(DbContextOptions<WebhooksDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Audit> Audits { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Audit>()
                .Property(x => x.CreatedUtc)
                .HasDefaultValue(DateTime.UtcNow);

            modelBuilder.Entity<Subscription>()
                .Property(x => x.CreatedUtc)
                .HasDefaultValue(DateTime.UtcNow);

            modelBuilder.Entity<Subscription>()
                .HasIndex(x => new { x.EventName, x.WebhookUri })
                .IsUnique();
        }
    }
}
