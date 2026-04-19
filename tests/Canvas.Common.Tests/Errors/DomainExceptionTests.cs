using Canvas.Common.Errors;
using FluentAssertions;

namespace Canvas.Common.Tests.Errors;

public class DomainExceptionTests
{
    [Fact]
    public void DomainException_ShouldBeAnException()
    {
        var ex = new DomainException("test");
        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void DomainException_ShouldPreserveMessage()
    {
        var ex = new DomainException("invariant violated");
        ex.Message.Should().Be("invariant violated");
    }
}
