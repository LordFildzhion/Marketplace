using FluentValidation;
using Marketplace.Application.DTOs.Products;

namespace Marketplace.Application.Validators.Products;

public sealed class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Title)
            .Length(3, 200)
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Stock.HasValue);
    }
}
