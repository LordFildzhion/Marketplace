namespace Marketplace.Application.Common.Exceptions;

public class ValidationException : AppException
{
    public IDictionary<string, string[]>? Errors { get; }
    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validation failed", "VALIDATION_ERROR")
    {
        Errors = errors;
    }
    public ValidationException(string message, IDictionary<string, string[]> errors)
        : base(message, "VALIDATION_ERROR")
    {
        Errors = errors;
    }
    public ValidationException(string property, string message)
        : base($"Validation failed: {message}", "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]> { { property, new[] { message } } };
    }
}
