
using Marketplace.Domain.Entities;

namespace Marketplace.Domain.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<(IReadOnlyList<Review> Reviews, int TotalCount)> GetProductReviewsAsync(
        Guid productId, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Review>> GetPendingReviewsAsync(CancellationToken ct = default);
}
