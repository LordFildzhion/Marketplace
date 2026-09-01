using Marketplace.Application.Common.Enums;

namespace Marketplace.Web.Models.Responses;

public sealed class ErrorResponse
{
    public string Message { get; init; } = string.Empty;

    public ErrorCode Code { get; init; }

    public object? Details { get; init; }

    public ErrorResponse(
        string message,
        ErrorCode code,
        object? details = null)
    {
        Message = message;
        Code = code;
        Details = details;
    }
}