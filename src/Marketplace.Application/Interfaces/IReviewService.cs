using Microsoft.AspNetCore.Http;
using Marketplace.Application.DTOs.Reviews;

namespace Marketplace.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> CreateReviewAsync(Guid productId, Guid userId, int rating, string comment, IFormFileCollection? images, CancellationToken ct = default);
    Task<IReadOnlyList<ReviewDto>> GetProductReviewsAsync(Guid productId, CancellationToken ct = default);
    Task<ReviewDto> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateReviewRequest request, CancellationToken ct = default);
    Task DeleteReviewAsync(Guid reviewId, Guid userId, bool isAdmin, CancellationToken ct = default);
    Task<ReviewDto> RespondToReviewAsync(Guid reviewId, Guid userId, string response, CancellationToken ct = default);
    Task<ReviewDto> DeleteResponseAsync(Guid reviewId, CancellationToken ct = default);
}
