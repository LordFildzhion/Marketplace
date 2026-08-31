using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

using Marketplace.Application.Interfaces;
using Marketplace.Application.Services;

namespace Marketplace.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // Application services
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPaymentService, PaymentService>();

        // Validators
        services.AddValidatorsFromAssembly(
            Assembly.GetExecutingAssembly());

        // AutoMapper
        services.AddAutoMapper(
            Assembly.GetExecutingAssembly());

        return services;
    }
}
