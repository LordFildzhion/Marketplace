
using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.BackgroundServices;

public class LowStockNotificationService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<LowStockNotificationService> _logger;
    private const int LowStockThreshold = 5;

    public LowStockNotificationService(IServiceProvider services, ILogger<LowStockNotificationService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var productRepo = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                var products = await productRepo.GetAllAsync(stoppingToken);
                var lowStock = products.Where(p => p.Stock < LowStockThreshold).ToList();

                foreach (var product in lowStock)
                {
                    _logger.LogWarning("Low stock alert: Product {ProductId} ({Title}) has {Stock} units left",
                        product.Id, product.Title, product.Stock);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking low stock");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
