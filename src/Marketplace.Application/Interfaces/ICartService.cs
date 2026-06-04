using Marketplace.Application.DTOs.Cart;

namespace Marketplace.Application.Interfaces;

public interface ICartService
{
    Task<IReadOnlyList<CartItemDto>> GetCartItemsAsync(Guid userId, CancellationToken ct = default);
    Task<CartItemDto> AddToCartAsync(Guid userId, AddToCartRequest request, CancellationToken ct = default);
    Task UpdateCartItemQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken ct = default);
    Task RemoveFromCartAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task ClearCartAsync(Guid userId, CancellationToken ct = default);
}
