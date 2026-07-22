using ATMS.Data.Enums;

namespace ATMS.Admin.Data.Entities.Onboarding;

public class OnboardingProgress
{
    public Guid UserId { get; set; }

    public User User { get; set; }

    public OnboardingStepStatusEnum PersonalInfoStatus { get; set; }

    public OnboardingStepStatusEnum SecurityStatus { get; set; }

    public OnboardingStepStatusEnum InvitationsStatus { get; set; }

    public string? PendingPasswordHash { get; set; }

    public DateTime UpdatedAt { get; set; }

    public long Version { get; set; }

    public OnboardingPersonalInfo? PersonalInfo { get; set; }

    public List<OnboardingInvitedUser> InvitedUsers { get; set; } = [];
}
