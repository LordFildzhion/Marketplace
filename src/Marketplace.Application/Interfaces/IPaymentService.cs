namespace Marketplace.Application.Interfaces;

public class PaymentResponseDto
{
    public bool IsSuccess { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
}

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessOrderPaymentAsync(Guid orderId, Guid userId, CancellationToken ct = default);
}
