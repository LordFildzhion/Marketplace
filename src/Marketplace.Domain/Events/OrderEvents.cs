using Marketplace.Domain.Common;

namespace Marketplace.Domain.Events;

public record OrderPaidEvent(Guid OrderId, Guid UserId) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid EventId { get; init; } = Guid.NewGuid();
}

public record OrderCancelledEvent(Guid OrderId, Guid UserId, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public Guid EventId { get; init; } = Guid.NewGuid();
}
