using Marketplace.Infrastructure.Data;
using Marketplace.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Marketplace.Infrastructure.Logging;

public class AuditLogger
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(AppDbContext context, ILogger<AuditLogger> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string entityName, Guid entityId, string action, string? oldValues = null, string? newValues = null, Guid? userId = null)
    {
        var audit = AuditLog.Create(entityName, entityId, action, oldValues, newValues, userId);
        await _context.AuditLogs.AddAsync(audit);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Audit: {Action} on {EntityName} [{EntityId}]", action, entityName, entityId);
    }
}
