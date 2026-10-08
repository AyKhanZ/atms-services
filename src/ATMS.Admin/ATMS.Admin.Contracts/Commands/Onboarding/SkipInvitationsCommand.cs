using ATMS.Admin.Contracts.Models.Onboarding;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Onboarding;

public sealed class SkipInvitationsCommand : IRequest<OnboardingModel>
{
    public long Version { get; set; }
}
