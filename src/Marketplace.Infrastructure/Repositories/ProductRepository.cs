using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Marketplace.Infrastructure.Data;

namespace Marketplace.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    private readonly ICategoryRepository _categoryRepository;

    public ProductRepository(AppDbContext context, ICategoryRepository categoryRepository)
    {
        _context = context;
        _categoryRepository = categoryRepository;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Products.FindAsync(new object[] { id }, ct);

    public async Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IReadOnlyList<Product> Products, int TotalCount)> SearchAsync(
        string? query, Guid? categoryId, decimal? minPrice, decimal? maxPrice,
        int page, int pageSize, string? sortBy, string? sortOrder, CancellationToken ct = default)
    {
        var q = _context.Products
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Include(p => p.Seller)   // <-- добавили загрузку продавца
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(p => EF.Functions.ILike(p.Title, $"%{query}%"));

        if (categoryId.HasValue)
        {
            var categoryIds = await _categoryRepository.GetAllSubcategoryIdsAsync(categoryId.Value, ct);
            categoryIds.Add(categoryId.Value);
            q = q.Where(p => categoryIds.Contains(p.CategoryId));
        }

        if (minPrice.HasValue)
            q = q.Where(p => p.Price.Amount >= minPrice.Value);
        if (maxPrice.HasValue)
            q = q.Where(p => p.Price.Amount <= maxPrice.Value);

        q = sortBy?.ToLower() switch
        {
            "title" => sortOrder == "desc" ? q.OrderByDescending(p => p.Title) : q.OrderBy(p => p.Title),
            "price" => sortOrder == "desc" ? q.OrderByDescending(p => p.Price.Amount) : q.OrderBy(p => p.Price.Amount),
            _ => q.OrderBy(p => p.CreatedAt)
        };

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Products.ToListAsync(ct);

    public async Task<Product> AddAsync(Product entity, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(Product entity, CancellationToken ct = default)
    {
        _context.Products.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product entity, CancellationToken ct = default)
    {
        _context.Products.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await _context.Products.AnyAsync(p => p.Id == id, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);

    public async Task AddImageToProductAsync(ProductImage image, CancellationToken ct = default)
    {
        await _context.Set<ProductImage>().AddAsync(image, ct);
    }

    public async Task RemoveImageAsync(Guid imageId, CancellationToken ct = default)
    {
        var image = await _context.Set<ProductImage>().FindAsync(new object[] { imageId }, ct);
        if (image != null)
            _context.Set<ProductImage>().Remove(image);
    }
}
