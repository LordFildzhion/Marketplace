using AutoMapper;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.DTOs.Common;
using Marketplace.Application.DTOs.Products;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Marketplace.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(id, ct);
        if (product == null) throw new NotFoundException("Product", id);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductSearchRequest request, CancellationToken ct = default)
    {
        var (items, total) = await _productRepository.SearchAsync(
            request.Query, request.CategoryId, request.MinPrice, request.MaxPrice,
            request.Page, request.PageSize, request.SortBy, request.SortOrder, ct);
        return new PagedResult<ProductDto>(_mapper.Map<List<ProductDto>>(items), total, request.Page, request.PageSize);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request, Guid sellerId, CancellationToken ct = default)
    {
        var skuStr = request.Sku;
        if (string.IsNullOrWhiteSpace(skuStr))
            skuStr = "AUTO-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        var sku = new Sku(skuStr);
        var price = new Money(request.Price, "USD");
        var product = new Product(sku, request.Title, request.Description, price, request.Stock, request.CategoryId, sellerId);
        await _productRepository.AddAsync(product, ct);
        await _productRepository.SaveChangesAsync(ct);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product == null) throw new NotFoundException("Product", id);

        if (!string.IsNullOrEmpty(request.Title) || !string.IsNullOrEmpty(request.Description))
            product.UpdateInfo(request.Title ?? product.Title, request.Description ?? product.Description);
        if (request.Price.HasValue)
        {
            var newPrice = new Money(request.Price.Value, product.Price.Currency);
            product.UpdateInfo(product.Title, product.Description, newPrice);
        }
        if (request.Stock.HasValue)
            product.ChangeStock(request.Stock.Value);

        if (request.CategoryId.HasValue && request.CategoryId.Value != Guid.Empty)
        {
            var categoryExists = await _categoryRepository.ExistsAsync(request.CategoryId.Value, ct);
            if (!categoryExists)
                throw new NotFoundException("Category", request.CategoryId.Value);

            product.ChangeCategory(request.CategoryId.Value);
        }

        await _productRepository.UpdateAsync(product, ct);
        await _productRepository.SaveChangesAsync(ct);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteProductAsync(Guid id, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(id, ct);
        if (product == null) throw new NotFoundException("Product", id);
        if (!isAdmin && product.SellerId != userId)
            throw new ForbiddenException("You can only delete your own products.");
        await _productRepository.DeleteAsync(product, ct);
        await _productRepository.SaveChangesAsync(ct);
    }

    public async Task<Guid> AddProductImageAsync(Guid productId, string imageUrl, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdWithDetailsAsync(productId, ct);
        if (product == null) throw new NotFoundException("Product", productId);
        var sortOrder = product.Images.Count + 1;
        var image = new ProductImage(imageUrl, false, sortOrder);
        image.ProductId = productId;
        await _productRepository.AddImageToProductAsync(image, ct);
        await _productRepository.SaveChangesAsync(ct);
        return image.Id;
    }

    public async Task RemoveProductImageAsync(Guid productId, Guid imageId, CancellationToken ct = default)
    {
        await _productRepository.RemoveImageAsync(imageId, ct);
        await _productRepository.SaveChangesAsync(ct);
    }

    public async Task AdjustStockAsync(Guid productId, int delta, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(productId, ct);
        if (product == null) throw new NotFoundException("Product", productId);
        var newStock = product.Stock + delta;
        if (newStock < 0)
            throw new AppException("Stock cannot be negative.");
        product.ChangeStock(newStock);
        await _productRepository.UpdateAsync(product, ct);
        await _productRepository.SaveChangesAsync(ct);
    }
}
