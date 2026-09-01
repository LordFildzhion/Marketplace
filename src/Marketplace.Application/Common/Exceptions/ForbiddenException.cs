using Marketplace.Application.Common.Enums;

namespace Marketplace.Application.Common.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message)
        : base(message, ErrorCode.Forbidden)
    {
    }

    public ForbiddenException()
        : base("Forbidden", ErrorCode.Forbidden)
    {
    }
}