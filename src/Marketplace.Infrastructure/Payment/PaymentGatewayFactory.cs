
using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Infrastructure.Payment;

public static class PaymentGatewayFactory
{
    public static IExternalPaymentGateway Create(IServiceProvider provider, bool useReal)
    {
        return useReal
            ? provider.GetRequiredService<StripePaymentGateway>()
            : provider.GetRequiredService<MockPaymentGateway>();
    }
}
