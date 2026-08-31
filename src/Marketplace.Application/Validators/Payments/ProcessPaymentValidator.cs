using FluentValidation;

namespace Marketplace.Application.Validators.Payments;

public sealed class ProcessPaymentValidator : AbstractValidator<ProcessPaymentRequest>
{
    public ProcessPaymentValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();
    }
}

public class ProcessPaymentRequest
{
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = "Mock";
}
