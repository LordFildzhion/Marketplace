using Marketplace.Infrastructure.Messaging;
using Marketplace.Application.Interfaces;
using Marketplace.Application.Services;
using Marketplace.Domain.Interfaces;
using Marketplace.Infrastructure.Auth;
using Marketplace.Infrastructure.BackgroundServices;
using Marketplace.Infrastructure.Data;
using Marketplace.Infrastructure.Logging;
using Marketplace.Infrastructure.Payment;
using Marketplace.Infrastructure.Repositories;
using Marketplace.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        services.AddScoped<DatabaseSeeder>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        services.AddScoped<MockPaymentGateway>();
        services.AddScoped<StripePaymentGateway>();
        services.AddScoped<IExternalPaymentGateway>(sp =>
        {
            var useReal = config.GetValue<bool>("Payment:UseRealGateway");
            return PaymentGatewayFactory.Create(sp, useReal);
        });

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<CurrentUserService>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddSingleton<IMessageBus, RabbitMqMessageBus>();

        services.AddScoped<AuditLogger>();

        services.AddHostedService<LowStockNotificationService>();
        services.AddHostedService<OrderProcessingService>();
            services.AddHostedService<OrderCreatedConsumer>();
            services.AddHostedService<order_status_changedConsumer>();
            services.AddHostedService<user_loggedinConsumer>();
            services.AddHostedService<user_registeredConsumer>();
            services.AddHostedService<review_response_addedConsumer>();
            services.AddHostedService<review_createdConsumer>();
            services.AddHostedService<product_deletedConsumer>();
            services.AddHostedService<product_updatedConsumer>();
            services.AddHostedService<product_createdConsumer>();

        return services;
    }
}
