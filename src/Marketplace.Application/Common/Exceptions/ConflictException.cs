using Marketplace.Application.Common.Enums;

namespace Marketplace.Application.Common.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, ErrorCode.Conflict)
    {
    }

    public ConflictException(
        string entity,
        string field,
        object value)
        : base(
            $"Conflict: {entity} with {field} '{value}' already exists.",
            ErrorCode.Conflict)
    {
    }
}