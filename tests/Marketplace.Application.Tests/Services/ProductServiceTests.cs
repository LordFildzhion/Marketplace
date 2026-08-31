using AutoMapper;
using FluentAssertions;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.Common.Mappings;
using Marketplace.Application.DTOs.Products;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Marketplace.Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly IMapper _mapper;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _service = new ProductService(_productRepo.Object, _categoryRepo.Object, _mapper, Mock.Of<ILogger<ProductService>>());
    }

    private static Product CreateProduct(int stock = 10)
        => new(new Sku("SKU"), "Title", "Desc", new Money(100, "USD"), stock, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task GetProductById_ExistingId_ShouldReturnDto()
    {
        var product = CreateProduct();
        _productRepo.Setup(r => r.GetByIdWithDetailsAsync(product.Id, default)).ReturnsAsync(product);
        var result = await _service.GetProductByIdAsync(product.Id);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Title");
    }

    [Fact]
    public async Task GetProductById_NotFound_ShouldThrow()
    {
        _productRepo.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetProductByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetProducts_ShouldPassSearchParameters()
    {
        var request = new ProductSearchRequest { Query = "phone", Page = 2, PageSize = 10, SortBy = "Price", SortOrder = "desc" };
        _productRepo.Setup(r => r.SearchAsync("phone", null, null, null, 2, 10, "Price", "desc", default))
            .ReturnsAsync((Array.Empty<Product>(), 0));

        var result = await _service.GetProductsAsync(request);
        result.TotalCount.Should().Be(0);
        _productRepo.Verify(r => r.SearchAsync("phone", null, null, null, 2, 10, "Price", "desc", default), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_ShouldAddAndReturnDto()
    {
        var request = new CreateProductRequest { Sku = "SKU", Title = "New", Description = "Desc", Price = 50, Stock = 5, CategoryId = Guid.NewGuid() };
        _productRepo.Setup(r => r.AddAsync(It.IsAny<Product>(), default)).ReturnsAsync((Product p, CancellationToken _) => p);
        var result = await _service.CreateProductAsync(request, Guid.NewGuid());
        result.Title.Should().Be("New");
        _productRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_EmptySku_ShouldGenerateSku()
    {
        var request = new CreateProductRequest { Sku = null, Title = "New", Description = "Desc", Price = 50, Stock = 5, CategoryId = Guid.NewGuid() };
        _productRepo.Setup(r => r.AddAsync(It.IsAny<Product>(), default)).ReturnsAsync((Product p, CancellationToken _) => p);
        var result = await _service.CreateProductAsync(request, Guid.NewGuid());
        result.Sku.Should().StartWith("AUTO-");
    }

    [Fact]
    public async Task UpdateProduct_ShouldChangeSuppliedFields()
    {
        var product = CreateProduct();
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        var result = await _service.UpdateProductAsync(product.Id, new UpdateProductRequest { Title = "Updated", Price = 125, Stock = 4, CategoryId = Guid.NewGuid() });
        result.Title.Should().Be("Updated");
        result.Price.Should().Be(125);
        result.Stock.Should().Be(4);
        _productRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateProduct_NotFound_ShouldThrow()
    {
        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Product?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateProductAsync(Guid.NewGuid(), new UpdateProductRequest()));
    }

    [Fact]
    public async Task DeleteProduct_NonOwner_ShouldThrowForbidden()
    {
        var product = CreateProduct();
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        await Assert.ThrowsAsync<ForbiddenException>(() => _service.DeleteProductAsync(product.Id, Guid.NewGuid(), false));
    }

    [Fact]
    public async Task DeleteProduct_Owner_ShouldDelete()
    {
        var product = CreateProduct();
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        await _service.DeleteProductAsync(product.Id, product.SellerId, false);
        _productRepo.Verify(r => r.DeleteAsync(product, default), Times.Once);
        _productRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AddProductImage_ProductMissing_ShouldThrow()
    {
        var id = Guid.NewGuid();
        _productRepo.Setup(r => r.GetByIdWithDetailsAsync(id, default)).ReturnsAsync((Product?)null);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.AddProductImageAsync(id, "img.jpg"));
    }

    [Fact]
    public async Task AddProductImage_ShouldPersistImage()
    {
        var product = CreateProduct();
        _productRepo.Setup(r => r.GetByIdWithDetailsAsync(product.Id, default)).ReturnsAsync(product);
        var id = await _service.AddProductImageAsync(product.Id, "img.jpg");
        id.Should().NotBeEmpty();
        _productRepo.Verify(r => r.AddImageToProductAsync(It.Is<ProductImage>(i => i.Url == "img.jpg" && i.ProductId == product.Id), default), Times.Once);
        _productRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RemoveProductImage_ShouldCallRepository()
    {
        var productId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        await _service.RemoveProductImageAsync(productId, imageId);
        _productRepo.Verify(r => r.RemoveImageAsync(imageId, default), Times.Once);
        _productRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task AdjustStock_NegativeResult_ShouldThrow()
    {
        var product = CreateProduct(2);
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        await Assert.ThrowsAsync<AppException>(() => _service.AdjustStockAsync(product.Id, -3));
    }

    [Fact]
    public async Task AdjustStock_ValidDelta_ShouldPersist()
    {
        var product = CreateProduct(2);
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, default)).ReturnsAsync(product);
        await _service.AdjustStockAsync(product.Id, 3);
        product.Stock.Should().Be(5);
        _productRepo.Verify(r => r.UpdateAsync(product, default), Times.Once);
        _productRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
