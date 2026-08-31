using Marketplace.Domain.Events;
using Marketplace.Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.EventHandlers;

public class OrderPaidEventHandler : IDomainEventHandler<OrderPaidEvent>
{
    private readonly ILogger<OrderPaidEventHandler> _logger;

    public OrderPaidEventHandler(ILogger<OrderPaidEventHandler> logger) => _logger = logger;

    public Task HandleAsync(OrderPaidEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Заказ {OrderId} оплачен", domainEvent.OrderId);
        return Task.CompletedTask;
    }
}
