using MediatR;

namespace Canvas.Common.Events;

public abstract record DomainEvent : INotification
{
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
