using Canvas.Infrastructure.Shared.Behaviors;
using Canvas.Infrastructure.Shared.Events;
using Canvas.Infrastructure.Shared.Persistence;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Canvas.Infrastructure.Shared.Tests.Behaviors;

public sealed record UnitOfWorkTestRequest : IRequest<string>;

public class UnitOfWorkBehaviorTests
{
    private static CanvasDbContext CreateInMemoryContext() =>
        new(new DbContextOptionsBuilder<CanvasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task Handle_ShouldReturnResponseFromNext()
    {
        await using var context = CreateInMemoryContext();
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        var behavior = new UnitOfWorkBehavior<UnitOfWorkTestRequest, string>(context, dispatcher);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        var result = await behavior.Handle(new UnitOfWorkTestRequest(), next, default);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ShouldCallDispatcherAfterSave()
    {
        await using var context = CreateInMemoryContext();
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        var behavior = new UnitOfWorkBehavior<UnitOfWorkTestRequest, string>(context, dispatcher);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        await behavior.Handle(new UnitOfWorkTestRequest(), next, default);

        await dispatcher.Received(1).DispatchAsync(context, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallNext()
    {
        await using var context = CreateInMemoryContext();
        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        var behavior = new UnitOfWorkBehavior<UnitOfWorkTestRequest, string>(context, dispatcher);
        var called = false;
        RequestHandlerDelegate<string> next = () => { called = true; return Task.FromResult("ok"); };

        await behavior.Handle(new UnitOfWorkTestRequest(), next, default);

        called.Should().BeTrue();
    }
}
