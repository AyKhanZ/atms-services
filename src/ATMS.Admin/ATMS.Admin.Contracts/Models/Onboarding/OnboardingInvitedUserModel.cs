using ATMS.Application.Models;

namespace ATMS.Admin.Contracts.Models.Onboarding;

public class OnboardingInvitedUserModel : AuditUserModel
{
    public string Email { get; set; }
}
