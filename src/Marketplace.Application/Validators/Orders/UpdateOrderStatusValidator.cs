using FluentValidation;
using Marketplace.Domain.Enums;
using Marketplace.Application.DTOs.Orders;

namespace Marketplace.Application.Validators.Orders;

public sealed class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.NewStatus)
            .Must(status => Enum.IsDefined(typeof(OrderStatus), status))
            .WithMessage("Invalid order status.");
    }
}
