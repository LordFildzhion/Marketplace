
namespace Marketplace.Web.Models.Responses;

public class ErrorResponse
{
    public string Message { get; set; }
    public string Code { get; set; }
    public object? Details { get; set; }

    public ErrorResponse(string message, string code, object? details = null)
    {
        Message = message;
        Code = code;
        Details = details;
    }
}
