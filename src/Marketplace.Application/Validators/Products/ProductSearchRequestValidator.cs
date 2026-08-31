using FluentValidation;
using Marketplace.Application.DTOs.Products;

namespace Marketplace.Application.Validators.Products;

public sealed class ProductSearchRequestValidator : AbstractValidator<ProductSearchRequest>
{
    public ProductSearchRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue);
    }
}
