using Marketplace.Application.DTOs.Common;
using Marketplace.Application.DTOs.Products;

namespace Marketplace.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductSearchRequest request, CancellationToken ct = default);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request, Guid sellerId, CancellationToken ct = default);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task DeleteProductAsync(Guid id, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task<Guid> AddProductImageAsync(Guid productId, string imageUrl, CancellationToken ct = default);
    Task RemoveProductImageAsync(Guid productId, Guid imageId, CancellationToken ct = default);
    Task AdjustStockAsync(Guid productId, int delta, CancellationToken ct = default);
}
