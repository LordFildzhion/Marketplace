
namespace Marketplace.Web.Models.Responses;

public class ValidationErrorResponse : ErrorResponse
{
    public new IDictionary<string, string[]> Details { get; }

    public ValidationErrorResponse(IDictionary<string, string[]> errors)
        : base("Validation failed", "VALIDATION_ERROR", errors)
    {
        Details = errors;
    }
}
