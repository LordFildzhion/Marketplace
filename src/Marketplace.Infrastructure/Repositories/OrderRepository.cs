using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Infrastructure.Data;

namespace Marketplace.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    public OrderRepository(AppDbContext context) => _context = context;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Orders.FindAsync(new object[] { id }, ct);

    public async Task<Order?> GetOrderWithItemsAsync(Guid orderId, CancellationToken ct = default) =>
        await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Seller)  // ← загрузка продавца
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

    public async Task<IReadOnlyList<Order>> GetOrdersByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default) =>
        await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Seller)
            .Include(o => o.User)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Order>> GetOrdersBySellerAsync(Guid sellerId, int page, int pageSize, CancellationToken ct = default) =>
        await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Seller)
            .Include(o => o.User)
            .Where(o => o.Items.Any(i => i.Product.SellerId == sellerId))
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Order>> GetAllOrdersAsync(int page, int pageSize, CancellationToken ct = default) =>
        await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .Include(o => o.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Seller)
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<long> GetMaxUserOrderNumberAsync(Guid userId, CancellationToken ct = default)
    {
        var max = await _context.Orders
            .Where(o => o.UserId == userId)
            .MaxAsync(o => (long?)o.UserOrderNumber, ct);
        return max ?? 0;
    }

    public async Task<Order> AddAsync(Order entity, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(entity, ct);
        foreach (var item in entity.Items)
        {
            _context.Set<OrderItem>().Add(item);
        }
        return entity;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Orders.ToListAsync(ct);

    public Task UpdateAsync(Order entity, CancellationToken ct = default)
    {
        _context.Orders.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Order entity, CancellationToken ct = default)
    {
        _context.Orders.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await _context.Orders.AnyAsync(o => o.Id == id, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
