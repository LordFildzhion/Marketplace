using FluentAssertions;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.Events;
using Marketplace.Domain.ValueObjects;
using Xunit;

namespace Marketplace.Domain.Tests.Entities;

public class OrderTests
{
    [Fact] public void NewOrder_ShouldHaveStatusNew() => new Order(Guid.NewGuid()).Status.Should().Be(OrderStatus.New);

    [Fact]
    public void AddItem_ShouldIncreaseTotalAndMergeSameProduct()
    {
        var order = new Order(Guid.NewGuid());
        var productId = Guid.NewGuid();
        order.AddItem(productId, "Test", new Money(10, "USD"), 2);
        order.AddItem(productId, "Test", new Money(10, "USD"), 3);
        order.TotalAmount.Amount.Should().Be(50);
        order.Items.Should().ContainSingle();
        order.Items.Single().Quantity.Should().Be(5);
    }

    [Fact] public void RemoveItem_ShouldRecalculateTotal()
    {
        var order = new Order(Guid.NewGuid());
        var productId = Guid.NewGuid();
        order.AddItem(productId, "Test", new Money(10, "USD"), 2);
        order.RemoveItem(productId);
        order.Items.Should().BeEmpty();
        order.TotalAmount.Amount.Should().Be(0);
    }

    [Fact] public void MarkAsPaid_ShouldCreateEvent()
    {
        var order = new Order(Guid.NewGuid());
        order.MarkAsPaid();
        order.Status.Should().Be(OrderStatus.Paid);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderPaidEvent>();
    }

    [Fact] public void Cancel_ShouldCreateEvent()
    {
        var order = new Order(Guid.NewGuid());
        order.Cancel("test");
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderCancelledEvent>();
    }

    [Fact] public void SetStatus_InvalidTransition_ShouldThrow()
    {
        var order = new Order(Guid.NewGuid());
        Action act = () => order.SetStatus(OrderStatus.Delivered);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact] public void AddItem_AfterProcessing_ShouldThrow()
    {
        var order = new Order(Guid.NewGuid());
        order.MarkAsPaid();
        Action act = () => order.AddItem(Guid.NewGuid(), "Test", new Money(10, "USD"), 1);
        act.Should().Throw<InvalidOperationException>();
    }
}
