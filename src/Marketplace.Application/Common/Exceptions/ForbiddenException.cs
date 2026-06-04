namespace Marketplace.Application.Common.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message, "FORBIDDEN") { }
    public ForbiddenException() : base("Forbidden", "FORBIDDEN") { }
}
