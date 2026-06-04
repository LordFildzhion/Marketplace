using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Infrastructure.Data;

namespace Marketplace.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;
    public CategoryRepository(AppDbContext context) => _context = context;

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Categories.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Categories.ToListAsync(ct);

    public async Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken ct = default)
    {
        var all = await _context.Categories.Include(c => c.SubCategories).ToListAsync(ct);
        return all.Where(c => c.ParentCategoryId == null).ToList();
    }

    public async Task<IReadOnlyList<Category>> GetSubcategoriesAsync(Guid parentId, CancellationToken ct = default) =>
        await _context.Categories.Where(c => c.ParentCategoryId == parentId).ToListAsync(ct);

    public async Task<HashSet<Guid>> GetAllSubcategoryIdsAsync(Guid parentId, CancellationToken ct = default)
    {
        var ids = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(parentId);
        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            var children = await _context.Categories
                .Where(c => c.ParentCategoryId == currentId)
                .Select(c => c.Id)
                .ToListAsync(ct);
            foreach (var childId in children)
            {
                ids.Add(childId);
                queue.Enqueue(childId);
            }
        }
        return ids;
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var query = _context.Categories.Where(c => c.Name == name);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return !await query.AnyAsync(ct);
    }

    public async Task<bool> HasProductsAsync(Guid categoryId, CancellationToken ct = default)
    {
        var allCategoryIds = await GetAllSubcategoryIdsAsync(categoryId, ct);
        allCategoryIds.Add(categoryId);
        return await _context.Products.AnyAsync(p => allCategoryIds.Contains(p.CategoryId), ct);
    }

    public async Task<bool> HasSubcategoriesAsync(Guid categoryId, CancellationToken ct = default) =>
        await _context.Categories.AnyAsync(c => c.ParentCategoryId == categoryId, ct);

    public async Task<Category> AddAsync(Category entity, CancellationToken ct = default)
    {
        await _context.Categories.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(Category entity, CancellationToken ct = default)
    {
        _context.Categories.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Category entity, CancellationToken ct = default)
    {
        _context.Categories.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await _context.Categories.AnyAsync(c => c.Id == id, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
