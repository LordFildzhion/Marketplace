
namespace Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Marketplace.Domain.Enums;

public interface IExternalPaymentGateway
{
    Task<PaymentResult> ProcessPaymentAsync(Guid orderId, Money amount, CancellationToken ct = default);
    Task<PaymentResult> RefundPaymentAsync(Guid transactionId, Money? amount = null, CancellationToken ct = default);
    Task<PaymentStatus> GetPaymentStatusAsync(Guid transactionId, CancellationToken ct = default);
}

public record PaymentResult
{
    public bool IsSuccess { get; init; }
    public Guid? TransactionId { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime ProcessedAt { get; init; } = DateTime.UtcNow;

    public static PaymentResult Success(Guid transactionId) =>
        new() { IsSuccess = true, TransactionId = transactionId };
    public static PaymentResult Failure(string error) =>
        new() { IsSuccess = false, ErrorMessage = error };
}
