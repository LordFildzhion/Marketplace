namespace Marketplace.Application.DTOs.Products;

public class ImageDto
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public List<ImageDto> Images { get; set; } = new();
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
}

public class CreateProductRequest
{
    public string? Sku { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
}

public class UpdateProductRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
    public Guid? CategoryId { get; set; }
}

public class ProductSearchRequest
{
    public string? Query { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}

public class AdjustStockRequest
{
    public int Delta { get; set; }
}
