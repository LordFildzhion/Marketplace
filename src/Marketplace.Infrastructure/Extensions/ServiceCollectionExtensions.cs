using Marketplace.Application.Interfaces;
using Marketplace.Application.Services;
using Marketplace.Domain.Events;
using Marketplace.Domain.Interfaces;
using Marketplace.Infrastructure.Auth;
using Marketplace.Infrastructure.BackgroundServices;
using Marketplace.Infrastructure.Data;
using Marketplace.Infrastructure.EventHandlers;
using Marketplace.Infrastructure.Events;
using Marketplace.Infrastructure.Logging;
using Marketplace.Infrastructure.Payment;
using Marketplace.Infrastructure.Repositories;
using Marketplace.Infrastructure.Storage;
using Marketplace.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Marketplace.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Domain events
        services.AddScoped<DomainEventInterceptor>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddScoped<
            IDomainEventHandler<ProductCreatedEvent>,
            ProductCreatedEventHandler>();

        services.AddScoped<
            IDomainEventHandler<UserRegisteredEvent>,
            UserRegisteredEventHandler>();

        services.AddScoped<
            IDomainEventHandler<OrderCancelledEvent>,
            OrderCancelledEventHandler>();

        services.AddScoped<
            IDomainEventHandler<OrderPaidEvent>,
            OrderPaidEventHandler>();

        // Database
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                config.GetConnectionString("DefaultConnection"));

            options.AddInterceptors(
                sp.GetRequiredService<DomainEventInterceptor>());
        });

        services.AddScoped<DatabaseSeeder>();

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Payment
        services.AddScoped<IExternalPaymentGateway, MockPaymentGateway>();

        // Storage
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // Auth
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<CurrentUserService>();

        // Messaging
        services.AddScoped<IMessageBus, RabbitMqMessageBus>();

        // Logging
        services.AddScoped<AuditLogger>();

        // Background services
        services.AddHostedService<LowStockNotificationService>();

        return services;
    }
}