using System.Text.Json;
using Ecommerce.Application.Abstractions;
using Ecommerce.Domain.Catalog;
using Ecommerce.Domain.Primitives;
using Ecommerce.Domain.Vendors;
using Ecommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    public DbSet<Vendor> Vendors => Set<Vendor>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        builder.Entity<ApplicationUser>().ToTable("users", "identity");
        builder.Entity<IdentityRole<Guid>>().ToTable("roles", "identity");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles", "identity");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", "identity");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", "identity");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims", "identity");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", "identity");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddDomainEventsToOutbox();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddDomainEventsToOutbox()
    {
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var type = domainEvent.GetType();
                OutboxMessages.Add(new OutboxMessage(
                    type.FullName ?? type.Name,
                    JsonSerializer.Serialize(domainEvent, type),
                    DateTimeOffset.UtcNow));
            }

            aggregate.ClearDomainEvents();
        }
    }
}
