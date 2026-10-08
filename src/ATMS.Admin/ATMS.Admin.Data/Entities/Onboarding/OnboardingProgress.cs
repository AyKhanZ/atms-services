
namespace ATMS.Admin.Data.Entities.Onboarding;

public class OnboardingProgress
{
    public Guid UserId { get; set; }

    public User User { get; set; }

    public int PersonalInfoStatus { get; set; }

    public int SecurityStatus { get; set; }

    public int InvitationsStatus { get; set; }

    public string? PendingPasswordHash { get; set; }

    public DateTime UpdatedAt { get; set; }

    public long Version { get; set; }

    public OnboardingPersonalInfo? PersonalInfo { get; set; }

    public ICollection<OnboardingInvitedUser> InvitedUsers { get; set; } = [];
}
