
using Polly;
using Polly.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Infrastructure.Extensions;

public static class PollyExtensions
{
    public static IServiceCollection AddPollyPolicies(this IServiceCollection services)
    {
        services.AddHttpClient("PaymentGateway")
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));
        return services;
    }
}
