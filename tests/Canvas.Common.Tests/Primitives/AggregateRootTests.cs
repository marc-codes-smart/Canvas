using Canvas.Common.Events;
using Canvas.Common.Primitives;
using FluentAssertions;

namespace Canvas.Common.Tests.Primitives;

public class AggregateRootTests
{
    private sealed class TestAggregate(int id) : AggregateRoot<int>(id)
    {
        public void RaiseTestEvent() => RaiseDomainEvent(new TestDomainEvent());
    }

    private sealed record TestDomainEvent() : DomainEvent;

    [Fact]
    public void AggregateRoot_ShouldStoreId()
    {
        var agg = new TestAggregate(1);
        agg.Id.Should().Be(1);
    }

    [Fact]
    public void AggregateRoot_ShouldBeAnEntity()
    {
        var agg = new TestAggregate(1);
        agg.Should().BeAssignableTo<Entity<int>>();
    }

    [Fact]
    public void AggregateRoot_ShouldAccumulateDomainEvents()
    {
        var agg = new TestAggregate(1);
        agg.RaiseTestEvent();
        agg.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void AggregateRoot_PopDomainEvents_ShouldReturnAndClear()
    {
        var agg = new TestAggregate(1);
        agg.RaiseTestEvent();

        var events = agg.PopDomainEvents();

        events.Should().HaveCount(1);
        agg.DomainEvents.Should().BeEmpty();
    }
}
