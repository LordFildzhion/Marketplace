using Marketplace.Application.Common.Enums;

namespace Marketplace.Application.Common.Exceptions;

public class ValidationException : AppException
{
    public IDictionary<string, string[]>? Errors { get; }

    public ValidationException(
        IDictionary<string, string[]> errors)
        : base("Validation failed", ErrorCode.Validation)
    {
        Errors = errors;
    }

    public ValidationException(
        string message,
        IDictionary<string, string[]> errors)
        : base(message, ErrorCode.Validation)
    {
        Errors = errors;
    }

    public ValidationException(
        string property,
        string message)
        : base(
            $"Validation failed: {message}",
            ErrorCode.Validation)
    {
        Errors = new Dictionary<string, string[]>
        {
            { property, new[] { message } }
        };
    }
}