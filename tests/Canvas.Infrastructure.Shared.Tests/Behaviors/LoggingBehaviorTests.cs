using Canvas.Infrastructure.Shared.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace Canvas.Infrastructure.Shared.Tests.Behaviors;

public sealed record LoggingTestRequest : IRequest<string>;

public class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldReturnResponseFromNext()
    {
        var logger = NullLogger<LoggingBehavior<LoggingTestRequest, string>>.Instance;
        var behavior = new LoggingBehavior<LoggingTestRequest, string>(logger);
        RequestHandlerDelegate<string> next = () => Task.FromResult("response");

        var result = await behavior.Handle(new LoggingTestRequest(), next, default);

        result.Should().Be("response");
    }

    [Fact]
    public async Task Handle_ShouldCallNext()
    {
        var logger = NullLogger<LoggingBehavior<LoggingTestRequest, string>>.Instance;
        var behavior = new LoggingBehavior<LoggingTestRequest, string>(logger);
        var called = false;
        RequestHandlerDelegate<string> next = () => { called = true; return Task.FromResult("ok"); };

        await behavior.Handle(new LoggingTestRequest(), next, default);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenNextThrows_ShouldRethrow()
    {
        var logger = NullLogger<LoggingBehavior<LoggingTestRequest, string>>.Instance;
        var behavior = new LoggingBehavior<LoggingTestRequest, string>(logger);
        RequestHandlerDelegate<string> next = () => throw new InvalidOperationException("fail");

        var act = () => behavior.Handle(new LoggingTestRequest(), next, default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
