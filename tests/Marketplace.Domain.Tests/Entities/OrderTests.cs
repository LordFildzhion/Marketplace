using FluentAssertions;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.ValueObjects;
using Xunit;

namespace Marketplace.Domain.Tests.Entities;

public class OrderTests
{
    [Fact]
    public void NewOrder_ShouldHaveStatusNew()
    {
        var order = new Order(Guid.NewGuid());
        order.Status.Should().Be(OrderStatus.New);
    }

    [Fact]
    public void AddItem_ShouldIncreaseTotal()
    {
        var order = new Order(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), "Test", new Money(10, "USD"), 2);
        order.TotalAmount.Amount.Should().Be(20);
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void SetStatus_ShouldAllowValidTransition()
    {
        var order = new Order(Guid.NewGuid());
        order.SetStatus(OrderStatus.Paid);
        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void SetStatus_InvalidTransition_ShouldThrow()
    {
        var order = new Order(Guid.NewGuid());
        Action act = () => order.SetStatus(OrderStatus.Delivered);
        act.Should().Throw<InvalidOperationException>();
    }
}
