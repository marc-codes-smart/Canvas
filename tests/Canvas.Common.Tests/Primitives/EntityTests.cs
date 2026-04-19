using Canvas.Common.Events;
using Canvas.Common.Primitives;
using FluentAssertions;

namespace Canvas.Common.Tests.Primitives;

public class EntityTests
{
    private sealed class TestEntity(int id) : Entity<int>(id);

    private sealed class TestEntityWithEvents(int id) : Entity<int>(id)
    {
        public void RaiseTestEvent() => RaiseDomainEvent(new TestDomainEvent());
    }

    private sealed record TestDomainEvent() : DomainEvent;

    [Fact]
    public void Id_ShouldBeStoredCorrectly()
    {
        var entity = new TestEntity(42);
        entity.Id.Should().Be(42);
    }

    [Fact]
    public void Equals_SameId_ShouldBeEqual()
    {
        var e1 = new TestEntity(1);
        var e2 = new TestEntity(1);
        e1.Should().Be(e2);
    }

    [Fact]
    public void Equals_DifferentId_ShouldNotBeEqual()
    {
        var e1 = new TestEntity(1);
        var e2 = new TestEntity(2);
        e1.Should().NotBe(e2);
    }

    [Fact]
    public void EqualityOperator_SameId_ShouldBeTrue()
    {
        var e1 = new TestEntity(1);
        var e2 = new TestEntity(1);
        (e1 == e2).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_DifferentId_ShouldBeTrue()
    {
        var e1 = new TestEntity(1);
        var e2 = new TestEntity(2);
        (e1 != e2).Should().BeTrue();
    }

    [Fact]
    public void RaiseDomainEvent_ShouldAccumulateEvents()
    {
        var entity = new TestEntityWithEvents(1);
        entity.RaiseTestEvent();
        entity.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void PopDomainEvents_ShouldReturnAllEvents()
    {
        var entity = new TestEntityWithEvents(1);
        entity.RaiseTestEvent();
        entity.RaiseTestEvent();

        var popped = entity.PopDomainEvents();
        popped.Should().HaveCount(2);
    }

    [Fact]
    public void PopDomainEvents_ShouldClearTheEventList()
    {
        var entity = new TestEntityWithEvents(1);
        entity.RaiseTestEvent();

        entity.PopDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }
}
