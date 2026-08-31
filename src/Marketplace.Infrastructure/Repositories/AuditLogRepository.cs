using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Entities;
using Marketplace.Domain.Interfaces;
using Marketplace.Infrastructure.Data;

namespace Marketplace.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;
    public AuditLogRepository(AppDbContext context) => _context = context;

    public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<AuditLog>> GetAllAsync(CancellationToken ct = default) =>
        await _context.AuditLogs.OrderByDescending(a => a.Timestamp).ToListAsync(ct);

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityName,
        Guid entityId,
        CancellationToken ct = default) =>
        await _context.AuditLogs
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(ct);

    public async Task<AuditLog> AddAsync(AuditLog entity, CancellationToken ct = default)
    {
        await _context.AuditLogs.AddAsync(entity, ct);
        return entity;
    }

    public Task UpdateAsync(AuditLog entity, CancellationToken ct = default)
    {
        _context.AuditLogs.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(AuditLog entity, CancellationToken ct = default)
    {
        _context.AuditLogs.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default) =>
        await _context.AuditLogs.AnyAsync(a => a.Id == id, ct);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);
}
