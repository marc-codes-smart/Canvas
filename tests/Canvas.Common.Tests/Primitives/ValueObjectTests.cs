using Canvas.Common.Primitives;
using FluentAssertions;

namespace Canvas.Common.Tests.Primitives;

public class ValueObjectTests
{
    private sealed class Money(decimal amount, string currency) : ValueObject
    {
        public decimal Amount { get; } = amount;
        public string Currency { get; } = currency;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void Equals_SameComponents_ShouldBeEqual()
    {
        var m1 = new Money(10m, "USD");
        var m2 = new Money(10m, "USD");
        m1.Should().Be(m2);
    }

    [Fact]
    public void Equals_DifferentAmount_ShouldNotBeEqual()
    {
        var m1 = new Money(10m, "USD");
        var m2 = new Money(20m, "USD");
        m1.Should().NotBe(m2);
    }

    [Fact]
    public void Equals_DifferentCurrency_ShouldNotBeEqual()
    {
        var m1 = new Money(10m, "USD");
        var m2 = new Money(10m, "EUR");
        m1.Should().NotBe(m2);
    }

    [Fact]
    public void GetHashCode_SameComponents_ShouldMatch()
    {
        var m1 = new Money(10m, "USD");
        var m2 = new Money(10m, "USD");
        m1.GetHashCode().Should().Be(m2.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_SameComponents_ShouldBeTrue()
    {
        var m1 = new Money(10m, "USD");
        var m2 = new Money(10m, "USD");
        (m1 == m2).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_DifferentComponents_ShouldBeTrue()
    {
        var m1 = new Money(10m, "USD");
        var m2 = new Money(20m, "USD");
        (m1 != m2).Should().BeTrue();
    }

    [Fact]
    public void Equals_NullOtherValueObject_ShouldNotBeEqual()
    {
        var m1 = new Money(10m, "USD");
        m1.Equals(null).Should().BeFalse();
    }
}
