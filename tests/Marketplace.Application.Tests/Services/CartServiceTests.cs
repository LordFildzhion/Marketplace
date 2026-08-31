using AutoMapper;
using FluentAssertions;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.Common.Mappings;
using Marketplace.Application.DTOs.Cart;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Marketplace.Application.Tests.Services;

public class CartServiceTests
{
    private readonly Mock<ICartRepository> _cartRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly IMapper _mapper;
    private readonly CartService _service;

    public CartServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
        _service = new CartService(_cartRepo.Object, _productRepo.Object, _mapper, Mock.Of<ILogger<CartService>>());
    }

    private static Product Product(int stock = 10)
        => new(new Sku("SKU"), "Phone", "Desc", new Money(100, "USD"), stock, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task GetCartItems_ShouldMapItems()
    {
        var userId = Guid.NewGuid();
        var product = Product();
        var item = new CartItem(userId, product.Id, 2);
        _cartRepo.Setup(r => r.GetCartItemsByUserAsync(userId, default)).ReturnsAsync(new[] { item });

        var result = await _service.GetCartItemsAsync(userId);

        result.Should().ContainSingle();
        result[0].Quantity.Should().Be(2);
        result[0].ProductId.Should().Be(product.Id);
    }

    [Fact]
    public async Task AddToCart_ProductNotFound_ShouldThrow()
    {
        var request = new AddToCartRequest { ProductId = Guid.NewGuid(), Quantity = 1 };
        _productRepo.Setup(r => r.GetByIdAsync(request.ProductId, default)).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AddToCartAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task AddToCart_NotEnoughStock_ShouldThrow()
    {
        var product = Product(1);
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);

        await Assert.ThrowsAsync<ConflictException>(() => _service.AddToCartAsync(
            Guid.NewGuid(), new AddToCartRequest { ProductId = product.Id, Quantity = 2 }));
    }

    [Fact]
    public async Task AddToCart_NewItem_ShouldAddAndSave()
    {
        var userId = Guid.NewGuid();
        var product = Product();
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        _cartRepo.Setup(r => r.GetCartItemAsync(userId, product.Id, default))
            .ReturnsAsync((CartItem?)null)
            .Callback(() => { });
        _cartRepo.Setup(r => r.AddAsync(It.IsAny<CartItem>(), default))
            .ReturnsAsync((CartItem c, CancellationToken _) => c);
        var created = new CartItem(userId, product.Id, 2);
        _cartRepo.SetupSequence(r => r.GetCartItemAsync(userId, product.Id, default))
            .ReturnsAsync((CartItem?)null)
            .ReturnsAsync(created);

        var result = await _service.AddToCartAsync(userId, new AddToCartRequest { ProductId = product.Id, Quantity = 2 });

        result.Quantity.Should().Be(2);
        _cartRepo.Verify(r => r.AddAsync(It.IsAny<CartItem>(), default), Times.Once);
        _cartRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Update_MissingItem_ShouldThrow()
    {
        _cartRepo.Setup(r => r.GetCartItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((CartItem?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateCartItemQuantityAsync(Guid.NewGuid(), Guid.NewGuid(), 2));
    }

    [Fact]
    public async Task Update_ExistingItem_ShouldUpdateAndSave()
    {
        var item = new CartItem(Guid.NewGuid(), Guid.NewGuid(), 1);
        _cartRepo.Setup(r => r.GetCartItemAsync(item.UserId, item.ProductId, default)).ReturnsAsync(item);

        await _service.UpdateCartItemQuantityAsync(item.UserId, item.ProductId, 3);

        item.Quantity.Should().Be(3);
        _cartRepo.Verify(r => r.UpdateAsync(item, default), Times.Once);
        _cartRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Remove_MissingItem_ShouldThrow()
    {
        _cartRepo.Setup(r => r.GetCartItemAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((CartItem?)null);
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.RemoveFromCartAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task Remove_ExistingItem_ShouldDeleteAndSave()
    {
        var item = new CartItem(Guid.NewGuid(), Guid.NewGuid(), 1);
        _cartRepo.Setup(r => r.GetCartItemAsync(item.UserId, item.ProductId, default)).ReturnsAsync(item);

        await _service.RemoveFromCartAsync(item.UserId, item.ProductId);

        _cartRepo.Verify(r => r.DeleteAsync(item, default), Times.Once);
        _cartRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Clear_ShouldCallRepository()
    {
        var userId = Guid.NewGuid();
        await _service.ClearCartAsync(userId);
        _cartRepo.Verify(r => r.ClearCartAsync(userId, default), Times.Once);
    }
}
