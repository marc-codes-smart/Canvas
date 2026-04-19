using Canvas.Infrastructure.Shared.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace Canvas.Infrastructure.Shared.Tests.Behaviors;

public sealed record ExceptionTestRequest : IRequest<string>;

public class ExceptionHandlingBehaviorTests
{
    [Fact]
    public async Task Handle_NoException_ReturnsResponse()
    {
        var logger = NullLogger<ExceptionHandlingBehavior<ExceptionTestRequest, string>>.Instance;
        var behavior = new ExceptionHandlingBehavior<ExceptionTestRequest, string>(logger);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        var result = await behavior.Handle(new ExceptionTestRequest(), next, default);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ExceptionThrown_RethrowsException()
    {
        var logger = NullLogger<ExceptionHandlingBehavior<ExceptionTestRequest, string>>.Instance;
        var behavior = new ExceptionHandlingBehavior<ExceptionTestRequest, string>(logger);
        RequestHandlerDelegate<string> next = () => throw new InvalidOperationException("oops");

        var act = () => behavior.Handle(new ExceptionTestRequest(), next, default);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("oops");
    }

    [Fact]
    public async Task Handle_ExceptionThrown_StillCallsNext()
    {
        var logger = NullLogger<ExceptionHandlingBehavior<ExceptionTestRequest, string>>.Instance;
        var behavior = new ExceptionHandlingBehavior<ExceptionTestRequest, string>(logger);
        var called = false;
        RequestHandlerDelegate<string> next = () =>
        {
            called = true;
            throw new Exception("fail");
        };

        try { await behavior.Handle(new ExceptionTestRequest(), next, default); } catch { }

        called.Should().BeTrue();
    }
}
