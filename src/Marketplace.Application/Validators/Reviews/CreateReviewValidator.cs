using FluentValidation;
using Marketplace.Application.DTOs.Reviews;

namespace Marketplace.Application.Validators.Reviews;

public sealed class CreateReviewValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .NotEmpty()
            .Length(3, 2000);
    }
}
