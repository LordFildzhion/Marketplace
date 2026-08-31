using FluentValidation;
using Marketplace.Application.DTOs.Cart;

namespace Marketplace.Application.Validators.Cart;

public sealed class UpdateCartItemValidator : AbstractValidator<UpdateCartItemRequest>
{
    public UpdateCartItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .InclusiveBetween(0, 100);
    }
}
