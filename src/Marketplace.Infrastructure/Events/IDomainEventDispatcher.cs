using Marketplace.Domain.Common;

namespace Marketplace.Infrastructure.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events);
}
