using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Messaging;
using ATMS.Messaging.Configuration;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Onboarding;

public sealed class CompleteOnboardingHandler(
    ICurrentUser currentUser,
    IOnboardingRepository onboardingRepository,
    IMapper mapper,
    IAccessTokenService accessTokenService,
    ICacheService cache,
    IOutboxRepository outboxRepository,
    IDictionariesRepository dictionariesRepository) : IRequestHandler<CompleteOnboardingCommand, OnboardingCompletionModel>
{
    public async Task<OnboardingCompletionModel> Handle(CompleteOnboardingCommand command, CancellationToken cancellationToken)
    {
        var progress = await onboardingRepository.GetAsync(currentUser.Id, cancellationToken)
            ?? throw new AuthException(AuthErrorTypeEnum.InvalidCredentials, LogMessages.InvalidCredentials);

        if (progress.User.HasCompletedOnboarding)
        {
            var existingUserToken = await accessTokenService.GenerateTokenAsync(
                progress.User,
                cancellationToken);
            
            await cache.RemoveAsync(CacheKeys.Admin.MeById(progress.User.Id), cancellationToken);
            await cache.RemoveAsync(CacheKeys.Admin.ProfileById(progress.User.Id), cancellationToken);
            
            return new OnboardingCompletionModel
            {
                AccessToken = existingUserToken.Token,
                AccessTokenExpireTime = existingUserToken.ExpiresInMinutes,
                InvitationsQueued = progress.InvitedUsers.Count
            };
        }

        var personalInfo = progress.PersonalInfo;
        var pendingPasswordHash = progress.PendingPasswordHash;
        var user = progress.User;

        mapper.Map(personalInfo, user);
        user.PasswordHash = pendingPasswordHash;
        user.HasCompletedOnboarding = true;
        user.OnboardingCompletedAt = DateTime.UtcNow;
        progress.PendingPasswordHash = null;

        var accessToken = await accessTokenService.GenerateTokenAsync(user, cancellationToken);

        await outboxRepository.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserUpdated,
            new UserUpdatedEvent(
                user.Id,
                user.Name,
                user.Surname,
                user.AvatarPath,
                user.HasCompletedOnboarding,
                user.Position,
                await LanguageCodeAsync(user.LanguageId, cancellationToken)),
            cancellationToken);

        foreach (var invitedUser in progress.InvitedUsers)
        {
            await outboxRepository.AddAsync(
                MessagingConstants.Exchanges.UserEvents,
                MessagingConstants.RoutingKeys.UserInvited,
                new UserInvitedEvent(
                    invitedUser.Email,
                    invitedUser.Name,
                    invitedUser.Surname,
                    user.OrganizationId,
                    user.Id),
                cancellationToken);
        }

        var saved = await onboardingRepository.TrySaveAsync(progress, command.Version, cancellationToken);
        if (!saved)
        {
            throw new ConflictException(OnboardingMessages.OnboardingConcurrencyConflict);
        }

        await cache.RemoveAsync(CacheKeys.Admin.MeById(user.Id), cancellationToken);
        await cache.RemoveAsync(CacheKeys.Admin.ProfileById(user.Id), cancellationToken);

        return new OnboardingCompletionModel
        {
            AccessToken = accessToken.Token,
            AccessTokenExpireTime = accessToken.ExpiresInMinutes,
            InvitationsQueued = progress.InvitedUsers.Count
        };
    }

    private async Task<string?> LanguageCodeAsync(int languageId, CancellationToken cancellationToken)
    {
        var languages = await dictionariesRepository.GetLanguagesAsync(cancellationToken);
        var code = languages.FirstOrDefault(language => language.Id == languageId)?.Code;
        return code is null ? null : SupportedLanguages.Normalize(code);
    }
}
