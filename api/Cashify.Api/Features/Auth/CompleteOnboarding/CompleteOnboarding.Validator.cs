using FluentValidation;

namespace Cashify.Api.Features.Auth.CompleteOnboarding;

public class CompleteOnboardingValidator : AbstractValidator<CompleteOnboardingRequest>
{
    public CompleteOnboardingValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BusinessName).MaximumLength(200);
    }
}
