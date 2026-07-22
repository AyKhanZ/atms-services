using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using AutoMapper;

namespace ATMS.Admin.Service.Mappers.Actions;

public class OnboardingModelMappingAction : IMappingAction<OnboardingProgress, OnboardingModel>
{
    private const int MaxInvitations = 10;

    public void Process(
        OnboardingProgress source,
        OnboardingModel destination,
        ResolutionContext context)
    {
        destination.Role = GetRoleCode(source);
        destination.CurrentStep = GetCurrentStep(source);
        destination.Steps = GetSteps(source);
        destination.MaxInvitations = MaxInvitations;
    }

    private static string GetRoleCode(OnboardingProgress progress)
    {
        return progress.User.UserRoles.First().RoleId switch
        {
            var roleId when roleId == RoleIds.ClientManager => "clientManager",
            var roleId when roleId == RoleIds.Client => "client",
            var roleId when roleId == RoleIds.Employee => "employee",
            var roleId when roleId == RoleIds.SuperAdmin => "superAdmin",
            var roleId => throw new InvalidOperationException(
                $"Role {roleId} is not supported by onboarding.")
        };
    }

    private static string GetCurrentStep(OnboardingProgress progress)
    {
        if (progress.User.HasCompletedOnboarding)
        {
            return "complete";
        }

        if (progress.PersonalInfoStatus != OnboardingStepStatusEnum.Completed)
        {
            return "personalInfo";
        }

        if (progress.SecurityStatus != OnboardingStepStatusEnum.Completed)
        {
            return "security";
        }

        var invitationsAvailable =
            progress.User.UserRoles.First().RoleId == RoleIds.ClientManager;
        if (invitationsAvailable &&
            progress.InvitationsStatus == OnboardingStepStatusEnum.NotStarted)
        {
            return "invitations";
        }

        return "review";
    }

    private static OnboardingStepModel[] GetSteps(OnboardingProgress progress)
    {
        var steps = new List<OnboardingStepModel>
        {
            CreateStep("personalInfo", progress.PersonalInfoStatus, true),
            CreateStep("security", progress.SecurityStatus, true)
        };

        if (progress.User.UserRoles.First().RoleId == RoleIds.ClientManager)
        {
            steps.Add(CreateStep("invitations", progress.InvitationsStatus, false));
        }

        return steps.ToArray();
    }

    private static OnboardingStepModel CreateStep(
        string code,
        OnboardingStepStatusEnum status,
        bool required)
    {
        return new OnboardingStepModel
        {
            Code = code,
            Status = GetStepStatus(status),
            Required = required
        };
    }

    private static string GetStepStatus(OnboardingStepStatusEnum status)
    {
        return status switch
        {
            OnboardingStepStatusEnum.Completed => "completed",
            OnboardingStepStatusEnum.Skipped => "skipped",
            _ => "notStarted"
        };
    }
}
