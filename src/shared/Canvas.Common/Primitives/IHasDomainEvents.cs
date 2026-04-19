using Canvas.Common.Events;

namespace Canvas.Common.Primitives;

public interface IHasDomainEvents
{
    IReadOnlyList<DomainEvent> DomainEvents { get; }
    IReadOnlyList<DomainEvent> PopDomainEvents();
}
