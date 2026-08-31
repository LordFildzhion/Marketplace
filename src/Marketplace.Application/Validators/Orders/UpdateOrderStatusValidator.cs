using FluentValidation;
using Marketplace.Application.DTOs.Orders;

namespace Marketplace.Application.Validators.Orders;

public sealed class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .Must(status => new[] { "Paid", "InProgress", "Shipped", "Delivered", "Cancelled" }.Contains(status));
    }
}
