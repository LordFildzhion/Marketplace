using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Infrastructure.Data;

namespace Marketplace.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly AppDbContext _context;
    public CartRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<CartItem>> GetCartItemsByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync(ct);
    }

    public async Task<CartItem?> GetCartItemAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId, ct);
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken ct = default)
    {
        var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync(ct);
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<CartItem?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.CartItems.FindAsync(new object[] { id }, ct);

    public async Task<IReadOnlyList<CartItem>> GetAllAsync(CancellationToken ct = default) =>
        await _context.CartItems.ToListAsync(ct);

    public async Task<CartItem> AddAsync(CartItem entity, CancellationToken ct = default)
    {
        await _context.CartItems.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(CartItem entity, CancellationToken ct = default)
    {
        _context.CartItems.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CartItem entity, CancellationToken ct = default)
    {
        _context.CartItems.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await _context.CartItems.AnyAsync(c => c.UserId == id && c.ProductId == id); // упрощено

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
