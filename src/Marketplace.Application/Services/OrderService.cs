using AutoMapper;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.DTOs.Orders;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Marketplace.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IMapper mapper,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrderDto> CreateOrderFromCartAsync(Guid userId, CancellationToken ct = default)
    {
        var cartItems = await _cartRepository.GetCartItemsByUserAsync(userId, ct);
        if (!cartItems.Any()) throw new AppException("Cart is empty");

        var order = new Order(userId);
        foreach (var item in cartItems)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
            if (product == null) throw new NotFoundException("Product", item.ProductId);
            if (product.Stock < item.Quantity)
                throw new ConflictException($"Not enough stock for {product.Title}");

            order.AddItem(product.Id, product.Title, product.CurrentPrice, item.Quantity);
            product.ChangeStock(product.Stock - item.Quantity);
            await _productRepository.UpdateAsync(product, ct);
        }

        var maxNumber = await _orderRepository.GetMaxUserOrderNumberAsync(userId, ct);
        order.SetUserOrderNumber(maxNumber + 1);

        await _orderRepository.AddAsync(order, ct);
        await _cartRepository.ClearCartAsync(userId, ct);
        await _orderRepository.SaveChangesAsync(ct);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId, ct);
        if (order == null) throw new NotFoundException("Order", orderId);
        return _mapper.Map<OrderDto>(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetUserOrdersAsync(Guid userId, CancellationToken ct = default)
    {
        var orders = await _orderRepository.GetOrdersByUserAsync(userId, 1, 50, ct);
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<IReadOnlyList<OrderDto>> GetSellerOrdersAsync(Guid sellerId, CancellationToken ct = default)
    {
        var orders = await _orderRepository.GetOrdersBySellerAsync(sellerId, 1, 50, ct);
        foreach (var order in orders)
        {
            var filteredItems = order.Items.Where(i => i.Product?.SellerId == sellerId).ToList();
            var itemsField = typeof(Order).GetField("_items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (itemsField != null)
            {
                var list = itemsField.GetValue(order) as List<OrderItem>;
                list.Clear();
                list.AddRange(filteredItems);
            }
        }
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(CancellationToken ct = default)
    {
        var orders = await _orderRepository.GetAllOrdersAsync(1, 100, ct);
        return _mapper.Map<List<OrderDto>>(orders);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, string newStatus, Guid userId, bool isAdmin, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        if (order == null) throw new NotFoundException("Order", orderId);

        if (!isAdmin && order.UserId != userId)
            throw new ForbiddenException("You can only update status of your own orders.");

        if (!Enum.TryParse<OrderStatus>(newStatus, true, out var status))
            throw new ValidationException("status", $"Invalid status: {newStatus}");

        order.SetStatus(status);
        await _orderRepository.UpdateAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);
        return _mapper.Map<OrderDto>(order);
    }
}
