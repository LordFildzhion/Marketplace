using Marketplace.Domain.Entities;

namespace Marketplace.Domain.Interfaces;

public interface ICartRepository : IRepository<CartItem>
{
    Task<IReadOnlyList<CartItem>> GetCartItemsByUserAsync(Guid userId, CancellationToken ct = default);
    Task<CartItem?> GetCartItemAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task ClearCartAsync(Guid userId, CancellationToken ct = default);
}
