using Marketplace.Domain.Entities;

namespace Marketplace.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<(IReadOnlyList<Product> Products, int TotalCount)> SearchAsync(
        string? query, Guid? categoryId, decimal? minPrice, decimal? maxPrice,
        int page, int pageSize, string? sortBy, string? sortOrder, CancellationToken ct = default);
    Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task AddImageToProductAsync(ProductImage image, CancellationToken ct = default);
    Task RemoveImageAsync(Guid imageId, CancellationToken ct = default);
}
