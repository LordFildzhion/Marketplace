using Marketplace.Domain.Common;
using Marketplace.Domain.Enums;
using Marketplace.Domain.ValueObjects;

namespace Marketplace.Domain.Entities;

public class Order : BaseEntity, IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public OrderStatus Status { get; private set; } = OrderStatus.New;
    public DateTime OrderDate { get; private set; } = DateTime.UtcNow;
    public Money TotalAmount { get; private set; } = null!;
    public Address? ShippingAddress { get; private set; }

    public long UserOrderNumber { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // for EF Core

    public Order(Guid userId)
    {
        UserId = userId;
        TotalAmount = new Money(0, "USD");
    }

    public void SetUserOrderNumber(long number)
    {
        if (number <= 0) throw new ArgumentException("UserOrderNumber must be positive");
        UserOrderNumber = number;
    }

    public void AddItem(Guid productId, string productTitle, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException("Cannot modify items after order is processed");

        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            existing.UpdateQuantity(existing.Quantity + quantity);
        }
        else
        {
            var newItem = new OrderItem(Id, productId, productTitle, unitPrice, quantity);
            newItem.Order = this;   // <-- устанавливаем обратную навигацию
            _items.Add(newItem);
        }
        RecalculateTotal();
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException("Cannot modify items after order is processed");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotal();
        }
    }

    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.New] = new[] { OrderStatus.Paid, OrderStatus.Cancelled },
        [OrderStatus.Paid] = new[] { OrderStatus.InProgress, OrderStatus.Cancelled },
        [OrderStatus.InProgress] = new[] { OrderStatus.Shipped, OrderStatus.Cancelled },
        [OrderStatus.Shipped] = new[] { OrderStatus.Delivered },
        [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
        [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
    };

    public void SetStatus(OrderStatus newStatus)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}.");

        Status = newStatus;
    }

    public void MarkAsPaid() => SetStatus(OrderStatus.Paid);
    public void StartProcessing() => SetStatus(OrderStatus.InProgress);
    public void Ship() => SetStatus(OrderStatus.Shipped);
    public void MarkAsDelivered() => SetStatus(OrderStatus.Delivered);
    public void Cancel() => SetStatus(OrderStatus.Cancelled);

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void RecalculateTotal()
    {
        var total = new Money(0, "USD");
        foreach (var item in _items)
            total = total.Add(item.TotalPrice);
        TotalAmount = total;
    }
}
