using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Data.Constants;
using ATMS.Data.Enums;

namespace ATMS.Admin.Service.Validation.Onboarding;

internal static class OnboardingStateValidator
{
    public static void EnsureNotCompleted(OnboardingProgress progress)
    {
        if (progress.User.HasCompletedOnboarding)
        {
            throw new ConflictException(OnboardingMessages.OnboardingAlreadyCompleted);
        }
    }

    public static (OnboardingPersonalInfo PersonalInfo, string PendingPasswordHash) EnsureCanComplete(OnboardingProgress progress)
    {
        EnsureNotCompleted(progress);

        var personalInfo = progress.PersonalInfo;
        if (progress.PersonalInfoStatus != OnboardingStepStatusEnum.Completed || personalInfo is null)
        {
            throw new ConflictException(OnboardingMessages.PersonalInfoIncomplete);
        }

        var pendingPasswordHash = progress.PendingPasswordHash;
        if (progress.SecurityStatus != OnboardingStepStatusEnum.Completed || pendingPasswordHash is null)
        {
            throw new ConflictException(OnboardingMessages.SecurityIncomplete);
        }

        if (progress.User.UserRoles.First().RoleId == RoleIds.ClientManager &&
            progress.InvitationsStatus == OnboardingStepStatusEnum.NotStarted)
        {
            throw new ConflictException(OnboardingMessages.InvitationsIncomplete);
        }

        return (personalInfo, pendingPasswordHash);
    }

    public static void EnsureInvitationsAvailable(OnboardingProgress progress)
    {
        EnsureNotCompleted(progress);

        if (progress.User.UserRoles.First().RoleId != RoleIds.ClientManager)
        {
            throw new ConflictException(OnboardingMessages.InvitationsManagerOnly);
        }
    }
}
