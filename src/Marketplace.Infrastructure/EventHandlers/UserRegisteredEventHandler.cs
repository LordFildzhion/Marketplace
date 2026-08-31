using Marketplace.Domain.Events;
using Marketplace.Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.EventHandlers;

public class UserRegisteredEventHandler : IDomainEventHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger) => _logger = logger;

    public Task HandleAsync(UserRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Новый пользователь: {Email}", domainEvent.Email);
        return Task.CompletedTask;
    }
}
