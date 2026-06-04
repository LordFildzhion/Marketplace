using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Infrastructure.Data;

namespace Marketplace.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;
    public ReviewRepository(AppDbContext context) => _context = context;

    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Review>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Reviews
            .Include(r => r.User)
            .ToListAsync(ct);

    public async Task<(IReadOnlyList<Review> Reviews, int TotalCount)> GetProductReviewsAsync(Guid productId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Reviews
            .Where(r => r.ProductId == productId)
            .Include(r => r.User);      // добавлено

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<Review>> GetPendingReviewsAsync(CancellationToken ct = default) =>
        await _context.Reviews
            .Include(r => r.User)
            .ToListAsync();

    public async Task<Review> AddAsync(Review entity, CancellationToken ct = default)
    {
        await _context.Reviews.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(Review entity, CancellationToken ct = default)
    {
        _context.Reviews.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Review entity, CancellationToken ct = default)
    {
        _context.Reviews.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await _context.Reviews.AnyAsync(r => r.Id == id, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
