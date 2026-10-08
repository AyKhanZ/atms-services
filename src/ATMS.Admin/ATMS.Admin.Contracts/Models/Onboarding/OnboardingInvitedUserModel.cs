using ATMS.Application.Models;

namespace ATMS.Admin.Contracts.Models.Onboarding;

public sealed class OnboardingInvitedUserModel : AuditUserModel
{
    public string Email { get; set; }
}
