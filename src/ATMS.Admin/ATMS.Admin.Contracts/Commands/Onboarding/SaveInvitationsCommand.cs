using ATMS.Admin.Contracts.Models.Onboarding;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Onboarding;

public class SaveInvitationsCommand : IRequest<OnboardingModel>
{
    public required List<InvitedUserCommand> Users { get; set; }
    
    public long Version { get; set; }
}
