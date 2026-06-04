
namespace Marketplace.Domain.Exceptions;

public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string rule)
        : base($"Business rule violation: {rule}") { }
}
