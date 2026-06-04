using AutoMapper;
using FluentAssertions;
using Moq;
using Marketplace.Application.DTOs.Orders;
using Marketplace.Application.Services;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Marketplace.Application.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock = new();
    private readonly Mock<ICartRepository> _cartRepoMock = new();
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly IMapper _mapper;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<Marketplace.Application.Common.Mappings.MappingProfile>());
        _mapper = config.CreateMapper();
        _orderService = new OrderService(
            _orderRepoMock.Object,
            _cartRepoMock.Object,
            _productRepoMock.Object,
            _mapper,
            Mock.Of<ILogger<OrderService>>());
    }

    [Fact]
    public async Task CreateOrderFromCart_EmptyCart_ShouldThrow()
    {
        _cartRepoMock.Setup(c => c.GetCartItemsByUserAsync(It.IsAny<Guid>(), default)).ReturnsAsync(new List<CartItem>());

        Func<Task> act = () => _orderService.CreateOrderFromCartAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<AppException>();
    }
}
