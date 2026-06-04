namespace Marketplace.Application.DTOs.Cart;
public class CartItemDto
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}
public class AddToCartRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
public class UpdateCartItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
