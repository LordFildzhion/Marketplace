using Marketplace.Domain.Entities;

namespace Marketplace.Domain.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetOrderWithItemsAsync(Guid orderId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetOrdersByUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetOrdersBySellerAsync(Guid sellerId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetAllOrdersAsync(int page, int pageSize, CancellationToken ct = default);
    Task<long> GetMaxUserOrderNumberAsync(Guid userId, CancellationToken ct = default);
}
