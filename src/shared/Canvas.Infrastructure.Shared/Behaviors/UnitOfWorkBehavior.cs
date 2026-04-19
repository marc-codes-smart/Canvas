using Canvas.Infrastructure.Shared.Events;
using Canvas.Infrastructure.Shared.Persistence;
using MediatR;

namespace Canvas.Infrastructure.Shared.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse>(
    CanvasDbContext dbContext,
    IDomainEventDispatcher dispatcher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();
        await dbContext.SaveChangesAsync(cancellationToken);
        await dispatcher.DispatchAsync(dbContext, cancellationToken);
        return response;
    }
}
