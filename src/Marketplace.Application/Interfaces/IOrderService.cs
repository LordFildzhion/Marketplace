using Marketplace.Application.DTOs.Orders;

namespace Marketplace.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderFromCartAsync(Guid userId, CancellationToken ct = default);
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetUserOrdersAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetSellerOrdersAsync(Guid sellerId, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(CancellationToken ct = default);
    Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, string status, Guid userId, bool isAdmin, CancellationToken ct = default);
}
