using Marketplace.Domain.Events;
using Marketplace.Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.EventHandlers;

public class OrderCancelledEventHandler : IDomainEventHandler<OrderCancelledEvent>
{
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(ILogger<OrderCancelledEventHandler> logger) => _logger = logger;

    public Task HandleAsync(OrderCancelledEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Заказ {OrderId} отменён: {Reason}", domainEvent.OrderId, domainEvent.Reason);
        return Task.CompletedTask;
    }
}
