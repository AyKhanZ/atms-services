using ATMS.Admin.Contracts.Models.Onboarding;
using MediatR;

namespace ATMS.Admin.Contracts.Commands.Onboarding;

public class SaveSecurityCommand : IRequest<OnboardingModel>
{
    public required string Password { get; set; }
    
    public required string ConfirmPassword { get; set; }
    
    public long Version { get; set; }
}
