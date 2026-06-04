namespace Marketplace.Application.Common.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string entity, object key) 
        : base($"Entity '{entity}' with key {key} not found.", "NOT_FOUND") { }
    public NotFoundException(string message) : base(message, "NOT_FOUND") { }
}
