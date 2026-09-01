using Marketplace.Application.Common.Enums;

namespace Marketplace.Application.Common.Exceptions;

public class AppException : Exception
{
    public ErrorCode ErrorCode { get; }

    public AppException(
        string message,
        ErrorCode errorCode = ErrorCode.Unknown)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public AppException(
        string message,
        Exception inner,
        ErrorCode errorCode = ErrorCode.Unknown)
        : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}