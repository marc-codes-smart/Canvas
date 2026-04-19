using Canvas.Common.Events;
using Canvas.Common.Primitives;
using Canvas.Infrastructure.Shared.Events;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Canvas.Infrastructure.Shared.Tests.Events;

public class DomainEventDispatcherTests
{
    private sealed class TestAggregate(int id) : AggregateRoot<int>(id)
    {
        public void RaiseEvent() => RaiseDomainEvent(new TestDomainEvent());
    }

    private sealed record TestDomainEvent() : DomainEvent;

    private sealed class TestDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAggregate>().HasKey(e => e.Id);
            modelBuilder.Entity<TestAggregate>().Ignore(a => a.DomainEvents);
        }
    }

    private static TestDbContext CreateContext() =>
        new(new DbContextOptionsBuilder()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task DispatchAsync_NoTrackedAggregates_PublishesNothing()
    {
        await using var context = CreateContext();
        var publisher = Substitute.For<IPublisher>();
        var dispatcher = new DomainEventDispatcher(publisher);

        await dispatcher.DispatchAsync(context);

        await publisher.DidNotReceiveWithAnyArgs().Publish(default!, default);
    }

    [Fact]
    public async Task DispatchAsync_TrackedAggregateWithEvents_PublishesEvents()
    {
        await using var context = CreateContext();
        var aggregate = new TestAggregate(1);
        aggregate.RaiseEvent();
        context.Aggregates.Add(aggregate);

        var publisher = Substitute.For<IPublisher>();
        var dispatcher = new DomainEventDispatcher(publisher);

        await dispatcher.DispatchAsync(context);

        await publisher.Received(1).Publish(Arg.Any<TestDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchAsync_ClearsDomainEvents_AfterPublishing()
    {
        await using var context = CreateContext();
        var aggregate = new TestAggregate(1);
        aggregate.RaiseEvent();
        context.Aggregates.Add(aggregate);

        var publisher = Substitute.For<IPublisher>();
        var dispatcher = new DomainEventDispatcher(publisher);

        await dispatcher.DispatchAsync(context);

        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task DispatchAsync_MultipleEvents_PublishesAll()
    {
        await using var context = CreateContext();
        var aggregate = new TestAggregate(1);
        aggregate.RaiseEvent();
        aggregate.RaiseEvent();
        context.Aggregates.Add(aggregate);

        var publisher = Substitute.For<IPublisher>();
        var dispatcher = new DomainEventDispatcher(publisher);

        await dispatcher.DispatchAsync(context);

        await publisher.Received(2).Publish(Arg.Any<TestDomainEvent>(), Arg.Any<CancellationToken>());
    }
}
