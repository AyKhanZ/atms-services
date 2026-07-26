using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Data.Entities.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Onboarding;

public sealed class SaveInvitationsHandler(
    ICurrentUser currentUser,
    IOnboardingRepository onboardingRepository,
    IMapper mapper) : IRequestHandler<SaveInvitationsCommand, OnboardingModel>
{
    public async Task<OnboardingModel> Handle(SaveInvitationsCommand command, CancellationToken cancellationToken)
    {
        var progress = await onboardingRepository.GetAsync(currentUser.Id, cancellationToken)
            ?? throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);

        progress.InvitedUsers.Clear();
        progress.InvitedUsers.AddRange(command.Users.Select(commandUser =>
        {
            var invitedUser = mapper.Map<OnboardingInvitedUser>(commandUser);
            invitedUser.OnboardingUserId = currentUser.Id;
            invitedUser.NormalizedEmail = invitedUser.Email.ToUpperInvariant();
            return invitedUser;
        }));
        progress.InvitationsStatus = OnboardingStepStatusEnum.Completed;

        var saved = await onboardingRepository.TrySaveAsync(progress, command.Version, cancellationToken);
        if (!saved)
        {
            throw new ConflictException(OnboardingMessages.OnboardingConcurrencyConflict);
        }

        return mapper.Map<OnboardingModel>(progress);
    }
}
