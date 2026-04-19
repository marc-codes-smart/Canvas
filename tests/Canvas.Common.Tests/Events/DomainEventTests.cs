using Canvas.Common.Events;
using FluentAssertions;

namespace Canvas.Common.Tests.Events;

public class DomainEventTests
{
    private sealed record TestDomainEvent() : DomainEvent;

    [Fact]
    public void OccurredOn_ShouldBeSetToUtcNowOnCreation()
    {
        var before = DateTimeOffset.UtcNow;
        var evt = new TestDomainEvent();
        var after = DateTimeOffset.UtcNow;

        evt.OccurredOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        var occurredOn = DateTimeOffset.UtcNow;
        var e1 = new TestDomainEvent() with { OccurredOn = occurredOn };
        var e2 = new TestDomainEvent() with { OccurredOn = occurredOn };

        e1.Should().Be(e2);
    }

    [Fact]
    public void RecordEquality_DifferentOccurredOn_ShouldNotBeEqual()
    {
        var e1 = new TestDomainEvent() with { OccurredOn = DateTimeOffset.UtcNow };
        var e2 = new TestDomainEvent() with { OccurredOn = DateTimeOffset.UtcNow.AddSeconds(1) };

        e1.Should().NotBe(e2);
    }
}
