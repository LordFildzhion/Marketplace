using Marketplace.Domain.Entities;

namespace Marketplace.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetTreeAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetSubcategoriesAsync(Guid parentId, CancellationToken ct = default);
    Task<HashSet<Guid>> GetAllSubcategoryIdsAsync(Guid parentId, CancellationToken ct = default);
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken ct = default);
    Task<bool> HasSubcategoriesAsync(Guid categoryId, CancellationToken ct = default);
}
