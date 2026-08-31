using Marketplace.Domain.Events;
using Marketplace.Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.EventHandlers;

public class ProductCreatedEventHandler : IDomainEventHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger) => _logger = logger;

    public Task HandleAsync(ProductCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Создан товар: {Title}", domainEvent.Title);
        return Task.CompletedTask;
    }
}
