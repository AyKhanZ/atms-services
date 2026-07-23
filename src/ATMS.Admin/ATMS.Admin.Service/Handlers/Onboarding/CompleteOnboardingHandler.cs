using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Onboarding;

public sealed class CompleteOnboardingHandler(
    ICurrentUser currentUser,
    IOnboardingRepository onboardingRepository,
    IMapper mapper,
    IAccessTokenService accessTokenService,
    ICacheService cache,
    IMessagePublisher messagePublisher) : IRequestHandler<CompleteOnboardingCommand, OnboardingCompletionModel>
{
    public async Task<OnboardingCompletionModel> Handle(CompleteOnboardingCommand command, CancellationToken cancellationToken)
    {
        var progress = await onboardingRepository.GetAsync(currentUser.Id, cancellationToken)
            ?? throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);

        if (progress.User.HasCompletedOnboarding)
        {
            var existingUserToken = await accessTokenService.GenerateTokenAsync(
                progress.User,
                cancellationToken);
            
            await cache.RemoveAsync(CacheKeys.Admin.MeById(progress.User.Id), cancellationToken);
            
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

        var saved = await onboardingRepository.TrySaveAsync(progress, command.Version, cancellationToken);
        if (!saved)
        {
            throw new ConflictException(OnboardingMessages.OnboardingConcurrencyConflict);
        }

        await cache.RemoveAsync(CacheKeys.Admin.MeById(user.Id), cancellationToken);

        await messagePublisher.PublishAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserUpdated,
            new UserUpdatedEvent(user.Id, user.Name, user.Surname, user.AvatarPath),
            cancellationToken);

        foreach (var invitedUser in progress.InvitedUsers)
        {
            await messagePublisher.PublishAsync(
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

        return new OnboardingCompletionModel
        {
            AccessToken = accessToken.Token,
            AccessTokenExpireTime = accessToken.ExpiresInMinutes,
            InvitationsQueued = progress.InvitedUsers.Count
        };
    }
}
