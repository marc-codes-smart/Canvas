namespace Canvas.Common.Events;

public abstract record DomainEvent
{
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
