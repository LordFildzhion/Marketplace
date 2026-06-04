
using Marketplace.Domain.Entities;

namespace Marketplace.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken ct = default);
}
