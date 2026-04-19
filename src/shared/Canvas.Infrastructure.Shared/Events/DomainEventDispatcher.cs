using Canvas.Common.Primitives;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Canvas.Infrastructure.Shared.Events;

public sealed class DomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    public async Task DispatchAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        var domainEvents = context.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .SelectMany(e => e.PopDomainEvents())
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
