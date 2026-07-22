namespace ATMS.Admin.Contracts.Models.Onboarding;

public class OnboardingModel
{
    public string Role { get; set; }

    public string CurrentStep { get; set; }

    public long Version { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool SecurityCompleted { get; set; }

    public OnboardingStepModel[] Steps { get; set; }

    public OnboardingPersonalInfoModel PersonalInfo { get; set; }

    public OnboardingInvitedUserModel[] InvitedUsers { get; set; }

    public int MaxInvitations { get; set; }
}
