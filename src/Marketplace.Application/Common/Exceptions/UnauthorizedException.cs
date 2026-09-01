using Marketplace.Application.Common.Enums;

namespace Marketplace.Application.Common.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message)
        : base(message, ErrorCode.Unauthorized)
    {
    }
}