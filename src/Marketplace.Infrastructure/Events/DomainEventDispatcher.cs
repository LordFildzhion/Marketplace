using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Marketplace.Domain.Common;

namespace Marketplace.Infrastructure.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events)
    {
        foreach (var domainEvent in events)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                try
                {
                    var method = handlerType.GetMethod("HandleAsync");
                    if (method != null)
                        await (Task)method.Invoke(handler, new object[] { domainEvent, CancellationToken.None })!;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при обработке события {EventName}", domainEvent.GetType().Name);
                }
            }
        }
    }
}
