using Marketplace.Domain.Common;

namespace Marketplace.Domain.Events;

public class UserRegisteredEvent : IDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public DateTime OccurredAt { get; }
    public Guid EventId { get; }

    public UserRegisteredEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
        OccurredAt = DateTime.UtcNow;
        EventId = Guid.NewGuid();
    }
}
