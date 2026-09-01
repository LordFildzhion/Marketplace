using Marketplace.Application.Common.Enums;

namespace Marketplace.Application.Common.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string entity, object key)
        : base(
            $"Entity '{entity}' with key {key} not found.",
            ErrorCode.NotFound)
    {
    }

    public NotFoundException(string message)
        : base(message, ErrorCode.NotFound)
    {
    }
}