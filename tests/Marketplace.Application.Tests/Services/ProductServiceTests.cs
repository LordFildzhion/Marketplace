using AutoMapper;
using FluentAssertions;
using Moq;
using Marketplace.Application.DTOs.Products;
using Marketplace.Application.Services;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Marketplace.Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly IMapper _mapper;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<Marketplace.Application.Common.Mappings.MappingProfile>());
        _mapper = config.CreateMapper();
        _productService = new ProductService(
            _productRepoMock.Object,
            _categoryRepoMock.Object,
            _mapper,
            Mock.Of<ILogger<ProductService>>());
    }

    [Fact]
    public async Task GetProductById_ExistingId_ShouldReturnDto()
    {
        var product = new Product(new Sku("SKU"), "Title", "Desc", new Money(100, "USD"), 10, Guid.NewGuid(), Guid.NewGuid());
        _productRepoMock.Setup(r => r.GetByIdWithDetailsAsync(product.Id, default)).ReturnsAsync(product);

        var result = await _productService.GetProductByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Title");
    }

    [Fact]
    public async Task CreateProduct_ShouldAddAndReturnDto()
    {
        var request = new CreateProductRequest { Sku = "SKU", Title = "New", Description = "Desc", Price = 50, Stock = 5, CategoryId = Guid.NewGuid() };
        _productRepoMock.Setup(r => r.AddAsync(It.IsAny<Product>(), default)).ReturnsAsync((Product p, CancellationToken _) => p);

        var result = await _productService.CreateProductAsync(request, Guid.NewGuid());

        result.Should().NotBeNull();
        _productRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
