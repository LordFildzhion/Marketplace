using Marketplace.Domain.Common;
using Marketplace.Domain.Enums;
using Marketplace.Domain.Events;
using Marketplace.Domain.ValueObjects;

namespace Marketplace.Domain.Entities;

public class User : BaseEntity, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsApproved { get; private set; } = false;
    public DateTime? LastLoginAt { get; private set; }

    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public User(Email email, string passwordHash, string firstName, string lastName, UserRole role = UserRole.Customer)
    {
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        IsApproved = role != UserRole.Seller;
        _domainEvents.Add(new UserRegisteredEvent(Id, email.Value));
    }

    public void UpdateProfile(string firstName, string lastName, string? phone)
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
    }

    public void ChangePassword(string newPasswordHash) => PasswordHash = newPasswordHash;
    public void SetRole(UserRole role) => Role = role;
    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void Approve() => IsApproved = true;
    public void Disapprove() => IsApproved = false;

    public void ClearDomainEvents() => _domainEvents.Clear();
    public string FullName => $"{FirstName} {LastName}".Trim();
}
