using Marketplace.Domain.Common;
using Marketplace.Domain.ValueObjects;

namespace Marketplace.Domain.Entities;

public class Review : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Rating Rating { get; private set; } = null!;
    public string Comment { get; private set; } = string.Empty;

    public string? SellerResponse { get; private set; }
    public DateTime? ResponseDate { get; private set; }
    public Guid? RespondedBy { get; private set; }

    public string ImageUrlsJson { get; private set; } = "[]";

    private Review() { }

    public Review(Guid productId, Guid userId, Rating rating, string comment)
    {
        ProductId = productId;
        UserId = userId;
        SetRating(rating);
        SetComment(comment);
    }

    public void SetComment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new ArgumentException("Comment is required");
        if (comment.Length < 3)
            throw new ArgumentException("Comment must be at least 3 characters");
        if (comment.Length > 2000)
            throw new ArgumentException("Comment cannot exceed 2000 characters");
        Comment = comment;
    }

    public void SetRating(Rating rating) => Rating = rating ?? throw new ArgumentNullException(nameof(rating));

    public void AddResponse(string response, Guid respondedBy)
    {
        SellerResponse = response;
        ResponseDate = DateTime.UtcNow;
        RespondedBy = respondedBy;
    }

    public void RemoveResponse()
    {
        SellerResponse = null;
        ResponseDate = null;
        RespondedBy = null;
    }

    public void SetImageUrls(List<string> urls)
    {
        ImageUrlsJson = System.Text.Json.JsonSerializer.Serialize(urls);
    }

    public List<string> GetImageUrls()
    {
        if (string.IsNullOrWhiteSpace(ImageUrlsJson))
            return new List<string>();
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(ImageUrlsJson) ?? new List<string>();
    }
}
