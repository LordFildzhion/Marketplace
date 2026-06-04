using Marketplace.Domain.Interfaces;
using Marketplace.Domain.ValueObjects;
using Marketplace.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.Payment;

public class StripePaymentGateway : IExternalPaymentGateway
{
    private readonly ILogger<StripePaymentGateway> _logger;
    public StripePaymentGateway(ILogger<StripePaymentGateway> logger) => _logger = logger;

    public Task<PaymentResult> ProcessPaymentAsync(Guid orderId, Money amount, CancellationToken ct = default)
    {
        _logger.LogInformation("Stripe processing payment for order {OrderId}, amount {Amount}", orderId, amount);
        throw new NotImplementedException();
    }

    public Task<PaymentResult> RefundPaymentAsync(Guid transactionId, Money? amount = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<PaymentStatus> GetPaymentStatusAsync(Guid transactionId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
