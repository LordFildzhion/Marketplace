using FluentValidation;
using Marketplace.Application.DTOs.Reviews;

namespace Marketplace.Application.Validators.Reviews;

public sealed class UpdateReviewValidator : AbstractValidator<UpdateReviewRequest>
{
    public UpdateReviewValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.Comment)
            .Length(3, 2000)
            .When(x => !string.IsNullOrEmpty(x.Comment));
    }
}
