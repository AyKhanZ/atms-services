using ATMS.Data;

namespace ATMS.Admin.Data.Entities.Onboarding;

public class OnboardingInvitedUser : UserBase
{
    public Guid OnboardingUserId { get; set; }

    public OnboardingProgress Progress { get; set; }

    public string NormalizedEmail { get; set; }
}
