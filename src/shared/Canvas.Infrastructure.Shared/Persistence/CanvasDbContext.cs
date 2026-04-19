using Canvas.Common.Primitives;
using Canvas.Infrastructure.Shared.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Canvas.Infrastructure.Shared.Persistence;

public class CanvasDbContext(DbContextOptions<CanvasDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CanvasDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasDomainEvents).IsAssignableFrom(entityType.ClrType))
                modelBuilder.Entity(entityType.ClrType).Ignore(nameof(IHasDomainEvents.DomainEvents));
        }

        base.OnModelCreating(modelBuilder);
    }
}
