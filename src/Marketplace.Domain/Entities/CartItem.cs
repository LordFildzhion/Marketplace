
using Marketplace.Domain.Common;

namespace Marketplace.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; }
    public int Quantity { get; private set; }
    public DateTime AddedAt { get; private set; } = DateTime.UtcNow;

    public CartItem(Guid userId, Guid productId, int quantity = 1)
    {
        UserId = userId;
        ProductId = productId;
        Quantity = quantity > 0 ? quantity : throw new ArgumentException("Quantity must be positive");
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive, use zero to remove");
        Quantity = newQuantity;
    }
    private CartItem() { }
}
