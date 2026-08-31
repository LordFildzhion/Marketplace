using FluentAssertions;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Marketplace.Application.Tests.Services;

public class PaymentServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IExternalPaymentGateway> _gateway = new();
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _service = new PaymentService(
            _orderRepo.Object,
            _gateway.Object,
            Mock.Of<ILogger<PaymentService>>());
    }

    [Fact]
    public async Task ProcessPayment_OrderNotFound_ShouldThrow()
    {
        _orderRepo.Setup(r => r.GetOrderWithItemsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.ProcessOrderPaymentAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task ProcessPayment_OtherUsersOrder_ShouldThrowForbidden()
    {
        var ownerId = Guid.NewGuid();
        var order = new Order(ownerId);
        _orderRepo.Setup(r => r.GetOrderWithItemsAsync(order.Id, default)).ReturnsAsync(order);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.ProcessOrderPaymentAsync(order.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task ProcessPayment_NonNewOrder_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        var order = new Order(userId);
        order.SetStatus(OrderStatus.Paid);
        _orderRepo.Setup(r => r.GetOrderWithItemsAsync(order.Id, default)).ReturnsAsync(order);

        await Assert.ThrowsAsync<AppException>(() =>
            _service.ProcessOrderPaymentAsync(order.Id, userId));
    }

    [Fact]
    public async Task ProcessPayment_Success_ShouldMarkOrderPaidAndSave()
    {
        var userId = Guid.NewGuid();
        var order = new Order(userId);
        var transactionId = Guid.NewGuid();
        _orderRepo.Setup(r => r.GetOrderWithItemsAsync(order.Id, default)).ReturnsAsync(order);
        _gateway.Setup(g => g.ProcessPaymentAsync(order.Id, order.TotalAmount, default))
            .ReturnsAsync(PaymentResult.Success(transactionId));

        var result = await _service.ProcessOrderPaymentAsync(order.Id, userId);

        result.IsSuccess.Should().BeTrue();
        result.TransactionId.Should().Be(transactionId.ToString());
        result.OrderStatus.Should().Be(nameof(OrderStatus.Paid));
        order.Status.Should().Be(OrderStatus.Paid);
        _orderRepo.Verify(r => r.UpdateAsync(order, default), Times.Once);
        _orderRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ProcessPayment_Failure_ShouldKeepOrderNewAndNotSave()
    {
        var userId = Guid.NewGuid();
        var order = new Order(userId);
        _orderRepo.Setup(r => r.GetOrderWithItemsAsync(order.Id, default)).ReturnsAsync(order);
        _gateway.Setup(g => g.ProcessPaymentAsync(order.Id, order.TotalAmount, default))
            .ReturnsAsync(PaymentResult.Failure("declined"));

        var result = await _service.ProcessOrderPaymentAsync(order.Id, userId);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("declined");
        result.OrderStatus.Should().Be(nameof(OrderStatus.New));
        _orderRepo.Verify(r => r.UpdateAsync(It.IsAny<Order>(), default), Times.Never);
        _orderRepo.Verify(r => r.SaveChangesAsync(default), Times.Never);
    }
}
