using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Marketplace.Web.Extensions;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddMarketplaceHealthChecks(this IServiceCollection services, IConfiguration config)
    {
        services.AddHealthChecks();
        return services;
    }
}
