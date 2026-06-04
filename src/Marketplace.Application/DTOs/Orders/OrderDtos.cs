namespace Marketplace.Application.DTOs.Orders;

public class OrderDto
{
    public Guid Id { get; set; }
    public long UserOrderNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? UserEmail { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerEmail { get; set; } = string.Empty;
    public string SellerNames { get; set; } = string.Empty;
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public ProductBriefDto? Product { get; set; }
}

public class ProductBriefDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();  // URL строки
    public string SellerName { get; set; } = string.Empty;
}

public class UpdateOrderStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
    public string? Comment { get; set; }
}
