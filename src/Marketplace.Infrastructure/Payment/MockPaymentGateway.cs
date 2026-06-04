using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Marketplace.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.Payment;

public class MockPaymentGateway : IExternalPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;
    public MockPaymentGateway(ILogger<MockPaymentGateway> logger) => _logger = logger;

    public Task<PaymentResult> ProcessPaymentAsync(Guid orderId, Money amount, CancellationToken ct = default)
    {
        _logger.LogInformation("Processing payment for order {OrderId}, amount {Amount}", orderId, amount);
        if (Random.Shared.Next(100) < 95)
        {
            var transactionId = Guid.NewGuid();
            _logger.LogInformation("Payment succeeded, transaction {TransactionId}", transactionId);
            return Task.FromResult(PaymentResult.Success(transactionId));
        }
        else
        {
            _logger.LogWarning("Payment failed for order {OrderId}", orderId);
            return Task.FromResult(PaymentResult.Failure("Insufficient funds"));
        }
    }

    public Task<PaymentResult> RefundPaymentAsync(Guid transactionId, Money? amount = null, CancellationToken ct = default)
    {
        _logger.LogInformation("Refunding transaction {TransactionId}, amount {Amount}", transactionId, amount);
        return Task.FromResult(PaymentResult.Success(Guid.NewGuid()));
    }

    public Task<PaymentStatus> GetPaymentStatusAsync(Guid transactionId, CancellationToken ct = default)
    {
        return Task.FromResult(PaymentStatus.Completed);
    }
}
