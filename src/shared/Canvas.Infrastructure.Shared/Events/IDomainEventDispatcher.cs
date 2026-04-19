using Microsoft.EntityFrameworkCore;

namespace Canvas.Infrastructure.Shared.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(DbContext context, CancellationToken cancellationToken = default);
}
