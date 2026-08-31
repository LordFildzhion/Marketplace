using FluentAssertions;
using Marketplace.Domain.Entities;
using Xunit;

namespace Marketplace.Domain.Tests.Entities;

public class CartItemTests
{
    [Fact]
    public void Constructor_InvalidQuantity_ShouldThrow()
    {
        Action act = () => new CartItem(Guid.NewGuid(), Guid.NewGuid(), 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateQuantity_Positive_ShouldChangeQuantity()
    {
        var item = new CartItem(Guid.NewGuid(), Guid.NewGuid(), 1);
        item.UpdateQuantity(4);
        item.Quantity.Should().Be(4);
    }

    [Fact]
    public void UpdateQuantity_Zero_ShouldThrow()
    {
        var item = new CartItem(Guid.NewGuid(), Guid.NewGuid(), 1);
        Action act = () => item.UpdateQuantity(0);
        act.Should().Throw<ArgumentException>();
    }
}
