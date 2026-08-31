using FluentValidation;
using Marketplace.Application.DTOs.Cart;

namespace Marketplace.Application.Validators.Cart;

public sealed class AddToCartValidator : AbstractValidator<AddToCartRequest>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}
