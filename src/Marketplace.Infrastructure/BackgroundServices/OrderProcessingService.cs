
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.BackgroundServices;

public class OrderProcessingService : BackgroundService
{
    private readonly ILogger<OrderProcessingService> _logger;

    public OrderProcessingService(ILogger<OrderProcessingService> logger) => _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Order processing cycle");
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
