using System.Security.Claims;
using Marketplace.Application.DTOs.Cart;
using Marketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Web.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize(Roles = "Customer")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService) => _cartService = cartService;

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CartItemDto>>> GetCart()
    {
        var items = await _cartService.GetCartItemsAsync(UserId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<CartItemDto>> AddToCart(AddToCartRequest request)
    {
        var item = await _cartService.AddToCartAsync(UserId, request);
        return Ok(item);
    }

    [HttpPut("{productId:guid}")]
    public async Task<ActionResult> UpdateCartItem(Guid productId, UpdateCartItemRequest request)
    {
        request.ProductId = productId; // убедимся, что совпадает
        await _cartService.UpdateCartItemQuantityAsync(UserId, productId, request.Quantity);
        return NoContent();
    }

    [HttpDelete("{productId:guid}")]
    public async Task<ActionResult> RemoveFromCart(Guid productId)
    {
        await _cartService.RemoveFromCartAsync(UserId, productId);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync(UserId);
        return NoContent();
    }
}
