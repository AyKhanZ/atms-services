using ATMS.Admin.Contracts.Models.Onboarding;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Onboarding;

public class CompleteOnboardingCommand : IRequest<OnboardingCompletionModel>
{
    public long Version { get; set; }
}
