using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using ValidationException = Marketplace.Application.Common.Exceptions.ValidationException;

namespace Marketplace.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .GroupBy(x => x.PropertyName, x => x.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.Distinct().ToArray());

        if (failures.Any())
        {
            _logger.LogWarning("Validation failed for {RequestType}. Errors: {@Errors}", typeof(TRequest).Name, failures);
            throw new ValidationException(failures);
        }

        _logger.LogDebug("Validation passed for {RequestType}", typeof(TRequest).Name);
        return await next();
    }
}
