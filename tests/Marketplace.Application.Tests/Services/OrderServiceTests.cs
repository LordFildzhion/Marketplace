using AutoMapper;
using FluentAssertions;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.Common.Mappings;
using Marketplace.Application.DTOs.Orders;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Marketplace.Application.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<ICartRepository> _cartRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly IMapper _mapper;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _service = new OrderService(_orderRepo.Object, _cartRepo.Object, _productRepo.Object, _mapper, Mock.Of<ILogger<OrderService>>());
    }

    [Fact]
    public async Task CreateOrderFromCart_EmptyCart_ShouldThrow()
    {
        _cartRepo.Setup(c => c.GetCartItemsByUserAsync(It.IsAny<Guid>(), default)).ReturnsAsync(Array.Empty<CartItem>());
        await Assert.ThrowsAsync<AppException>(() => _service.CreateOrderFromCartAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateOrderFromCart_MissingProduct_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        var item = new CartItem(userId, Guid.NewGuid(), 1);
        _cartRepo.Setup(c => c.GetCartItemsByUserAsync(userId, default)).ReturnsAsync(new[] { item });
        _productRepo.Setup(r => r.GetByIdAsync(item.ProductId, default)).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateOrderFromCartAsync(userId));
    }

    [Fact]
    public async Task CreateOrderFromCart_InsufficientStock_ShouldThrow()
    {
        var userId = Guid.NewGuid();
        var product = new Product(new Sku("SKU"), "Phone", "Desc", new Money(100, "USD"), 1, Guid.NewGuid(), Guid.NewGuid());
        var item = new CartItem(userId, product.Id, 2);
        _cartRepo.Setup(c => c.GetCartItemsByUserAsync(userId, default)).ReturnsAsync(new[] { item });
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateOrderFromCartAsync(userId));
    }

    [Fact]
    public async Task CreateOrderFromCart_ValidCart_ShouldCreateOrderReduceStockAndClearCart()
    {
        var userId = Guid.NewGuid();
        var product = new Product(new Sku("SKU"), "Phone", "Desc", new Money(100, "USD"), 10, Guid.NewGuid(), Guid.NewGuid());
        var item = new CartItem(userId, product.Id, 2);
        _cartRepo.Setup(c => c.GetCartItemsByUserAsync(userId, default)).ReturnsAsync(new[] { item });
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _orderRepo.Setup(r => r.GetMaxUserOrderNumberAsync(userId, default)).ReturnsAsync(7);
        _orderRepo.Setup(r => r.AddAsync(It.IsAny<Order>(), default)).ReturnsAsync((Order o, CancellationToken _) => o);

        var result = await _service.CreateOrderFromCartAsync(userId);

        result.UserOrderNumber.Should().Be(8);
        result.TotalAmount.Should().Be(200);
        product.Stock.Should().Be(8);
        _productRepo.Verify(r => r.UpdateAsync(product, default), Times.Once);
        _orderRepo.Verify(r => r.AddAsync(It.Is<Order>(o => o.Items.Count == 1), default), Times.Once);
        _cartRepo.Verify(r => r.ClearCartAsync(userId, default), Times.Once);
        _orderRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetOrderById_NotFound_ShouldThrow()
    {
        _orderRepo.Setup(r => r.GetOrderWithItemsAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Order?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetOrderByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetUserOrders_ShouldRequestFirst50()
    {
        var userId = Guid.NewGuid();
        _orderRepo.Setup(r => r.GetOrdersByUserAsync(userId, 1, 50, default)).ReturnsAsync(Array.Empty<Order>());
        var result = await _service.GetUserOrdersAsync(userId);
        result.Should().BeEmpty();
        _orderRepo.Verify(r => r.GetOrdersByUserAsync(userId, 1, 50, default), Times.Once);
    }

    [Fact]
    public async Task GetSellerOrders_ShouldRequestFirst50()
    {
        var sellerId = Guid.NewGuid();
        _orderRepo.Setup(r => r.GetOrdersBySellerAsync(sellerId, 1, 50, default)).ReturnsAsync(Array.Empty<Order>());
        (await _service.GetSellerOrdersAsync(sellerId)).Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllOrders_ShouldRequestFirst100()
    {
        _orderRepo.Setup(r => r.GetAllOrdersAsync(1, 100, default)).ReturnsAsync(Array.Empty<Order>());
        (await _service.GetAllOrdersAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateOrderStatus_OtherUser_ShouldThrowForbidden()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var order = new Order(ownerId);

        _orderRepo.Setup(r =>
            r.GetByIdAsync(order.Id, default))
            .ReturnsAsync(order);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _service.UpdateOrderStatusAsync(
                order.Id,
                "Paid",
                otherUserId,
                false));
    }
    
    [Fact]
    public async Task UpdateOrderStatus_InvalidStatus_ShouldThrowValidation()
    {
        var userId = Guid.NewGuid();
        var order = new Order(userId);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, default)).ReturnsAsync(order);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _service.UpdateOrderStatusAsync(order.Id, "Wrong", userId, false));
    }

    [Fact]
    public async Task UpdateOrderStatus_Admin_ShouldChangeStatus()
    {
        var order = new Order(Guid.NewGuid());
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, default)).ReturnsAsync(order);

        var result = await _service.UpdateOrderStatusAsync(order.Id, "Paid", Guid.NewGuid(), true);

        result.Status.Should().Be("Paid");
        order.Status.Should().Be(OrderStatus.Paid);
        _orderRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
