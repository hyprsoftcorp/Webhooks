using Microsoft.EntityFrameworkCore;
using Hyprsoft.Webhooks.Core.Management;
using System;

namespace Hyprsoft.Webhooks.Core.Hangfire
{
    public class WebhooksDbContext : DbContext
    {
        public WebhooksDbContext(DbContextOptions<WebhooksDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Subscription>()
                .Property(x => x.CreatedUtc)
                .HasDefaultValue(DateTime.UtcNow);

            modelBuilder.Entity<Subscription>()
                .HasIndex(x => new { x.TypeName, x.WebhookUri })
                .IsUnique();
        }
    }
}
