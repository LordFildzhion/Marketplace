using Marketplace.Domain.Common;

namespace Marketplace.Domain.Events;

public class OrderPaidEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public DateTime OccurredAt { get; }
    public Guid EventId { get; }

    public OrderPaidEvent(Guid orderId, Guid userId)
    {
        OrderId = orderId;
        UserId = userId;
        OccurredAt = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }
}
