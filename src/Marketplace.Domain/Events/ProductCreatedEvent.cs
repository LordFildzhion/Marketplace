using Marketplace.Domain.Common;

namespace Marketplace.Domain.Events;

public class ProductCreatedEvent : IDomainEvent
{
    public Guid ProductId { get; }
    public Guid SellerId { get; }
    public string Title { get; }
    public DateTime OccurredAt { get; }
    public Guid EventId { get; }

    public ProductCreatedEvent(Guid productId, Guid sellerId, string title)
    {
        ProductId = productId;
        SellerId = sellerId;
        Title = title;
        OccurredAt = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }
}
