namespace ATMS.Admin.Contracts.Models.Onboarding;

public sealed class OnboardingStepModel
{
    public string Code { get; set; }

    public string Status { get; set; }

    public bool Required { get; set; }
}
