namespace Marketplace.Application.Common.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, "CONFLICT") { }
    public ConflictException(string entity, string field, object value)
        : base($"Conflict: {entity} with {field} '{value}' already exists.", "CONFLICT") { }
}
