using Marketplace.Domain.Common;

namespace Marketplace.Infrastructure.Events;

public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task HandleAsync(T domainEvent, CancellationToken cancellationToken);
}
