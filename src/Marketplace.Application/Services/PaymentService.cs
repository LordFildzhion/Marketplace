using Marketplace.Application.Common.Exceptions;
using Marketplace.Application.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Enums;
using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Marketplace.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IExternalPaymentGateway _gateway;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IOrderRepository orderRepository, IExternalPaymentGateway gateway, ILogger<PaymentService> logger)
    {
        _orderRepository = orderRepository;
        _gateway = gateway;
        _logger = logger;
    }

    public async Task<PaymentResponseDto> ProcessOrderPaymentAsync(Guid orderId, Guid userId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetOrderWithItemsAsync(orderId, ct);
        if (order == null) throw new NotFoundException("Order", orderId);
        if (order.UserId != userId) throw new ForbiddenException("Cannot pay for another user's order");
        if (order.Status != OrderStatus.New) 
            throw new AppException($"Order cannot be paid in status {order.Status}");

        _logger.LogInformation("Processing payment for order {OrderId}, amount {Amount}", orderId, order.TotalAmount);
        var result = await _gateway.ProcessPaymentAsync(orderId, order.TotalAmount, ct);

        if (result.IsSuccess)
        {
            order.MarkAsPaid();
            await _orderRepository.UpdateAsync(order, ct);
            await _orderRepository.SaveChangesAsync(ct);
            _logger.LogInformation("Payment succeeded for order {OrderId}, tx {TransactionId}", orderId, result.TransactionId);
        }
        else
        {
            _logger.LogWarning("Payment failed for order {OrderId}: {Error}", orderId, result.ErrorMessage);
        }

        return new PaymentResponseDto
        {
            IsSuccess = result.IsSuccess,
            TransactionId = result.TransactionId?.ToString(),
            ErrorMessage = result.ErrorMessage,
            OrderStatus = order.Status.ToString()
        };
    }
}
