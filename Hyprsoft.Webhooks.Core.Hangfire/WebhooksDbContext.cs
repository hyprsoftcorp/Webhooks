using Microsoft.EntityFrameworkCore;
using Hyprsoft.Webhooks.Core.Management;
using System;

namespace Hyprsoft.Webhooks.Core.Hangfire
{
    public class WebhooksDbContext : DbContext
    {
        public const string WebhooksDbName = "WebhooksDb";

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
