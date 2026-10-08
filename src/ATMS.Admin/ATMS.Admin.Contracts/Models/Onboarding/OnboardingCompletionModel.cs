namespace ATMS.Admin.Contracts.Models.Onboarding;

public sealed class OnboardingCompletionModel
{
    public string AccessToken { get; set; }

    public DateTime AccessTokenExpireTime { get; set; }

    public int InvitationsQueued { get; set; }
}
