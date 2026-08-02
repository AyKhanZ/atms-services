using ATMS.Admin.Contracts.Models.Onboarding;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Onboarding;

public class SkipInvitationsCommand : IRequest<OnboardingModel>
{
    public long Version { get; set; }
}
