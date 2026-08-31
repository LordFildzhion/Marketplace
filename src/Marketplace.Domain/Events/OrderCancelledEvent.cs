using Marketplace.Domain.Common;

namespace Marketplace.Domain.Events;

public class OrderCancelledEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public Guid UserId { get; }
    public string Reason { get; }
    public DateTime OccurredAt { get; }
    public Guid EventId { get; }

    public OrderCancelledEvent(Guid orderId, Guid userId, string reason)
    {
        OrderId = orderId;
        UserId = userId;
        Reason = reason;
        OccurredAt = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }
}
