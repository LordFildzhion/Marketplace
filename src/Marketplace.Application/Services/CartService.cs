using AutoMapper;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.DTOs.Cart;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Marketplace.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CartService> _logger;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository, IMapper mapper, ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CartItemDto>> GetCartItemsAsync(Guid userId, CancellationToken ct = default)
    {
        var cartItems = await _cartRepository.GetCartItemsByUserAsync(userId, ct);
        return _mapper.Map<List<CartItemDto>>(cartItems);
    }

    public async Task<CartItemDto> AddToCartAsync(Guid userId, AddToCartRequest request, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct);
        if (product == null) throw new NotFoundException("Product", request.ProductId);
        if (!product.IsActive || product.Stock < request.Quantity)
            throw new ConflictException("Not enough stock or product not available");

        var existing = await _cartRepository.GetCartItemAsync(userId, request.ProductId, ct);
        if (existing != null)
        {
            existing.UpdateQuantity(existing.Quantity + request.Quantity);
            await _cartRepository.UpdateAsync(existing, ct);
        }
        else
        {
            var cartItem = new CartItem(userId, request.ProductId, request.Quantity);
            await _cartRepository.AddAsync(cartItem, ct);
        }

        await _cartRepository.SaveChangesAsync(ct);
        var updatedItem = await _cartRepository.GetCartItemAsync(userId, request.ProductId, ct);
        return _mapper.Map<CartItemDto>(updatedItem);
    }

    public async Task UpdateCartItemQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken ct = default)
    {
        var cartItem = await _cartRepository.GetCartItemAsync(userId, productId, ct);
        if (cartItem == null) throw new NotFoundException("CartItem", (userId, productId));
        cartItem.UpdateQuantity(quantity);
        await _cartRepository.UpdateAsync(cartItem, ct);
        await _cartRepository.SaveChangesAsync(ct);
    }

    public async Task RemoveFromCartAsync(Guid userId, Guid productId, CancellationToken ct = default)
    {
        var cartItem = await _cartRepository.GetCartItemAsync(userId, productId, ct);
        if (cartItem == null) throw new NotFoundException("CartItem", (userId, productId));
        await _cartRepository.DeleteAsync(cartItem, ct);
        await _cartRepository.SaveChangesAsync(ct);
    }

    public async Task ClearCartAsync(Guid userId, CancellationToken ct = default)
    {
        await _cartRepository.ClearCartAsync(userId, ct);
    }
}
