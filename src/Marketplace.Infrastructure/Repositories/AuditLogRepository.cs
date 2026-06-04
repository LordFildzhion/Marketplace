
using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Infrastructure.Data;

namespace Marketplace.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;
    public AuditLogRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, Guid entityId, CancellationToken ct = default) =>
        await _context.AuditLogs.Where(a => a.EntityName == entityName && a.EntityId == entityId).OrderByDescending(a => a.Timestamp).ToListAsync(ct);

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
    public async Task<IReadOnlyList<AuditLog>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();

    public async Task<AuditLog> AddAsync(AuditLog entity, CancellationToken ct = default)
    {
        await _context.AuditLogs.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(AuditLog entity, CancellationToken ct = default) => throw new NotImplementedException();
    public Task DeleteAsync(AuditLog entity, CancellationToken ct = default) => throw new NotImplementedException();
    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _context.SaveChangesAsync(ct);
}
