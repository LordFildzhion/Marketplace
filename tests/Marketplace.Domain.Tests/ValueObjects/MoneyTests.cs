using FluentAssertions;
using Marketplace.Domain.ValueObjects;
using Xunit;

namespace Marketplace.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var money = new Money(100, "USD");
        money.Amount.Should().Be(100);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Constructor_NegativeAmount_ShouldThrow()
    {
        Action act = () => new Money(-5, "USD");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_InvalidCurrency_ShouldThrow()
    {
        Action act = () => new Money(10, "US");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        var m1 = new Money(100, "USD");
        var m2 = new Money(50, "USD");
        var result = m1.Add(m2);
        result.Amount.Should().Be(150);
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrow()
    {
        var m1 = new Money(100, "USD");
        var m2 = new Money(50, "EUR");
        Action act = () => m1.Add(m2);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Equality_ShouldBeByValue()
    {
        var m1 = new Money(100, "USD");
        var m2 = new Money(100, "USD");
        m1.Should().Be(m2);
    }
}
