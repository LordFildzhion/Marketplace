using Marketplace.Domain.Common;
using Marketplace.Domain.ValueObjects;

namespace Marketplace.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Order Order { get; set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; set; } = null!;   // <-- добавлено
    public string ProductTitle { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = null!;
    public Money TotalPrice => UnitPrice.Multiply(Quantity);

    private OrderItem() { } // for EF Core

    public OrderItem(Guid orderId, Guid productId, string productTitle, Money unitPrice, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductTitle = productTitle;
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        UpdateQuantity(quantity);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive");
        Quantity = newQuantity;
    }
}
