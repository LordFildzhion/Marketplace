
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Web.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddMarketplaceAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("OrderOwner", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim(c => c.Type == "role" && c.Value == "Customer")));
        });
        return services;
    }
}
