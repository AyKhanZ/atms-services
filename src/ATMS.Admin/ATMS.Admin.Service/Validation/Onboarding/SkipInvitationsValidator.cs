using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Service.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Onboarding;

public sealed class SkipInvitationsValidator : AbstractValidator<SkipInvitationsCommand>
{
    public SkipInvitationsValidator()
    {
        RuleFor(x => x.Version)
            .GreaterThanOrEqualTo(0)
            .WithMessage(OnboardingMessages.VersionInvalid);
    }
}
