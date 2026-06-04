using Marketplace.Domain.Common;

namespace Marketplace.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string EntityName { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = null!;
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public Guid? ChangedBy { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    private AuditLog() { }

    public static AuditLog Create(string entityName, Guid entityId, string action,
        string? oldValues, string? newValues, Guid? changedBy)
    {
        return new AuditLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            ChangedBy = changedBy
        };
    }
}
