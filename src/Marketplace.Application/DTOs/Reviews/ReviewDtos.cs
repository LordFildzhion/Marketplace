namespace Marketplace.Application.DTOs.Reviews;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> ImageUrls { get; set; } = new();

    public string? SellerResponse { get; set; }
    public DateTime? ResponseDate { get; set; }
    public Guid? RespondedBy { get; set; }
}

public class CreateReviewRequest
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    // Файлы будут передаваться отдельно через IFormFileCollection в контроллере
}

public class UpdateReviewRequest
{
    public int? Rating { get; set; }
    public string? Comment { get; set; }
}

public class RespondToReviewRequest
{
    public string Response { get; set; } = string.Empty;
}
