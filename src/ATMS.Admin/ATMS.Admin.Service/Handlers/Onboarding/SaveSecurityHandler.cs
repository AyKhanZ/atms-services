using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Admin.Service.Validation.Onboarding;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Onboarding;

public sealed class SaveSecurityHandler(
    ICurrentUser currentUser,
    IOnboardingRepository onboardingRepository,
    IPasswordHasherService passwordHasherService,
    IMapper mapper) : IRequestHandler<SaveSecurityCommand, OnboardingModel>
{
    public async Task<OnboardingModel> Handle(SaveSecurityCommand command, CancellationToken cancellationToken)
    {
        var progress = await onboardingRepository.GetAsync(currentUser.Id, cancellationToken)
            ?? throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);
        OnboardingStateValidator.EnsureNotCompleted(progress);

        progress.PendingPasswordHash = passwordHasherService.Hash(command.Password);
        progress.SecurityStatus = OnboardingStepStatusEnum.Completed;

        var saved = await onboardingRepository.TrySaveAsync(
            progress,
            command.Version,
            cancellationToken);
        
        if (!saved)
        {
            throw new ConflictException(OnboardingMessages.OnboardingConcurrencyConflict);
        }

        return mapper.Map<OnboardingModel>(progress);
    }
}
