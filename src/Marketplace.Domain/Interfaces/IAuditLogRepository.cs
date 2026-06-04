
using Marketplace.Domain.Entities;

namespace Marketplace.Domain.Interfaces;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken ct = default);
}
