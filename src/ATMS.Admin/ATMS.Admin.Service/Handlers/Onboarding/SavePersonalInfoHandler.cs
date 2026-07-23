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
using ATMS.Infrastructure.Images;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Onboarding;

public sealed class SavePersonalInfoHandler(
    ICurrentUser currentUser,
    IOnboardingRepository onboardingRepository,
    IImageStorage imageStorage,
    IMapper mapper) : IRequestHandler<SavePersonalInfoCommand, OnboardingModel>
{
    public async Task<OnboardingModel> Handle(SavePersonalInfoCommand command, CancellationToken cancellationToken)
    {
        var progress = await onboardingRepository.GetAsync(currentUser.Id, cancellationToken)
            ?? throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);

        var oldAvatarPath = progress.PersonalInfo?.AvatarPath;
        string? newAvatarPath = null;

        if (command.Avatar is not null)
        {
            var image = await imageStorage.SaveAsync(
                command.Avatar,
                ImageStorageFolder.Users,
                currentUser.Id,
                cancellationToken);
            newAvatarPath = image.RelativePath;
        }

        var personalInfo = progress.PersonalInfo;
        if (personalInfo is null)
        {
            personalInfo = mapper.Map<OnboardingPersonalInfo>(command);
            personalInfo.Id = currentUser.Id;
            progress.PersonalInfo = personalInfo;
        }
        else
        {
            mapper.Map(command, personalInfo);
        }

        personalInfo.Email = progress.User.Email;
        personalInfo.AvatarPath = newAvatarPath ?? personalInfo.AvatarPath;
        progress.PersonalInfoStatus = OnboardingStepStatusEnum.Completed;

        try
        {
            var saved = await onboardingRepository.TrySaveAsync(progress, command.Version, cancellationToken);
            if (!saved)
            {
                throw new ConflictException(OnboardingMessages.OnboardingConcurrencyConflict);
            }
        }
        catch
        {
            if (newAvatarPath is not null)
            {
                await imageStorage.DeleteAsync(newAvatarPath, CancellationToken.None);
            }

            throw;
        }

        if (newAvatarPath is not null &&
            oldAvatarPath is not null &&
            oldAvatarPath != newAvatarPath)
        {
            await imageStorage.DeleteAsync(oldAvatarPath, cancellationToken);
        }

        return mapper.Map<OnboardingModel>(progress);
    }
}
