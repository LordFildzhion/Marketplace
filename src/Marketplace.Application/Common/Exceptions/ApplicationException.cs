namespace Marketplace.Application.Common.Exceptions;

public class AppException : Exception
{
    public string ErrorCode { get; }
    public AppException(string message, string errorCode = "UNKNOWN") : base(message)
    {
        ErrorCode = errorCode;
    }
    public AppException(string message, Exception inner, string errorCode = "UNKNOWN") : base(message, inner)
    {
        ErrorCode = errorCode;
    }
}
