using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Service.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Onboarding;

public sealed class CompleteOnboardingValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingValidator()
    {
        RuleFor(x => x.Version)
            .GreaterThanOrEqualTo(0).WithMessage(OnboardingMessages.VersionInvalid);
    }
}
