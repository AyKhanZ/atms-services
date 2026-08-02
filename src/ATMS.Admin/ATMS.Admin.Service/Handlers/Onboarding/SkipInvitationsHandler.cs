using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Contracts.Models.Onboarding;
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

public sealed class SkipInvitationsHandler(
    ICurrentUser currentUser,
    IOnboardingRepository onboardingRepository,
    IMapper mapper) : IRequestHandler<SkipInvitationsCommand, OnboardingModel>
{
    public async Task<OnboardingModel> Handle(SkipInvitationsCommand command, CancellationToken cancellationToken)
    {
        var progress = await onboardingRepository.GetAsync(currentUser.Id, cancellationToken)
            ?? throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);

        progress.InvitedUsers.Clear();
        progress.InvitationsStatus = OnboardingStepStatusEnum.Skipped;

        var saved = await onboardingRepository.TrySaveAsync(progress, command.Version, cancellationToken);
        if (!saved)
        {
            throw new ConflictException(OnboardingMessages.OnboardingConcurrencyConflict);
        }

        return mapper.Map<OnboardingModel>(progress);
    }
}
