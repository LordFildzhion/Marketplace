using FluentAssertions;
using FluentValidation;
using Marketplace.Application.Extensions;
using Marketplace.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Marketplace.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ShouldRegisterAllApplicationServices()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        services.Should().ContainSingle(x => x.ServiceType == typeof(IProductService) && x.ImplementationType == typeof(Marketplace.Application.Services.ProductService));
        services.Should().ContainSingle(x => x.ServiceType == typeof(IOrderService) && x.ImplementationType == typeof(Marketplace.Application.Services.OrderService));
        services.Should().ContainSingle(x => x.ServiceType == typeof(ICartService) && x.ImplementationType == typeof(Marketplace.Application.Services.CartService));
        services.Should().ContainSingle(x => x.ServiceType == typeof(IAuthService) && x.ImplementationType == typeof(Marketplace.Application.Services.AuthService));
        services.Should().ContainSingle(x => x.ServiceType == typeof(IReviewService) && x.ImplementationType == typeof(Marketplace.Application.Services.ReviewService));
        services.Should().ContainSingle(x => x.ServiceType == typeof(ICategoryService) && x.ImplementationType == typeof(Marketplace.Application.Services.CategoryService));
        services.Should().ContainSingle(x => x.ServiceType == typeof(IUserService) && x.ImplementationType == typeof(Marketplace.Application.Services.UserService));
        services.Should().ContainSingle(x => x.ServiceType == typeof(IPaymentService) && x.ImplementationType == typeof(Marketplace.Application.Services.PaymentService));
    }

    [Fact]
    public void AddApplication_ShouldRegisterValidators()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        services.Count(x =>
                x.ServiceType.IsGenericType &&
                x.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>))
            .Should().BeGreaterThanOrEqualTo(12);
    }
}
