
namespace Marketplace.Domain.Exceptions;

public class InvalidEntityStateException : DomainException
{
    public InvalidEntityStateException(string entityName, string reason)
        : base($"Entity {entityName} is in invalid state: {reason}") { }
}
