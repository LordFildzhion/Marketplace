using System.Security.Claims;
using Marketplace.Application.DTOs.Orders;
using Marketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Web.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService) => _orderService = orderService;

    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string UserRole => User.FindFirst(ClaimTypes.Role)!.Value;

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder()
    {
        var order = await _orderService.CreateOrderFromCartAsync(UserId);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return Ok(order);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrders()
    {
        if (UserRole == "Admin")
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        else if (UserRole == "Seller")
        {
            var orders = await _orderService.GetSellerOrdersAsync(UserId);
            return Ok(orders);
        }
        else
        {
            var orders = await _orderService.GetUserOrdersAsync(UserId);
            return Ok(orders);
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await _orderService.UpdateOrderStatusAsync(id, request.NewStatus, UserId, User.IsInRole("Admin"));
        return Ok(order);
    }
}
