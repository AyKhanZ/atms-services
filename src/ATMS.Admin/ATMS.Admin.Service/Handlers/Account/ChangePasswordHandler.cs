using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Interfaces;
using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Enums;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public sealed class ChangePasswordHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IUserSessionRepository userSessionRepository,
    IPasswordHasherService passwordHasherService,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    ICacheService cache) : IRequestHandler<ChangePasswordCommand, AccessInfoModel>
{
    public async Task<AccessInfoModel> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(u => u.Id == currentUser.Id, cancellationToken)
            ?? throw new EntityException(EntityErrorTypeEnum.NotFound, AccountMessages.UserNotFound);

        var now = DateTime.UtcNow;
        if (user.UserStatusId == (int)UserStatusEnum.Locked &&
            user.LockoutEnd.HasValue && user.LockoutEnd > now)
        {
            var remainingMinutes = Math.Ceiling((user.LockoutEnd.Value - now).TotalMinutes);
            throw new AuthException(AuthErrorTypeEnum.AccountLocked,
                string.Format(AuthMessages.AccountLocked, remainingMinutes));
        }

        if (user.UserStatusId == (int)UserStatusEnum.Inactive)
        {
            throw new AuthException(AuthErrorTypeEnum.AccountInactive, AuthMessages.AccountInactive);
        }

        if (!passwordHasherService.Verify(command.OldPassword, user.PasswordHash))
        {
            // counted in one sql statement, so parallel attempts can't skip the limit
            var lockedNow = await userRepository.RegisterFailedPasswordAsync(
                user.Id,
                5,
                now,
                now.AddMinutes(15),
                cancellationToken);

            if (lockedNow)
            {
                await RemoveUserCardAsync(user.Id, cancellationToken);
                throw new AuthException(AuthErrorTypeEnum.AccountLocked,
                    string.Format(AuthMessages.AccountLocked, 15));
            }

            throw new ValidationException([
                new ValidationFailure(nameof(command.OldPassword), AccountMessages.OldPasswordIncorrect)
            ]);
        }

        user.FailedLoginCount = 0;
        // a correct password ends the timed lock from wrong passwords
        if (user.UserStatusId == (int)UserStatusEnum.Locked && user.LockoutEnd.HasValue)
        {
            user.UserStatusId = (int)UserStatusEnum.Active;
        }

        user.LockoutEnd = null;
        user.PasswordHash = passwordHasherService.Hash(command.NewPassword);
        var expectedVersion = user.SessionVersion;
        user.SessionVersion = expectedVersion + 1;

        var accessToken = await accessTokenService.GenerateTokenAsync(user, cancellationToken);
        var refreshToken = await refreshTokenService.GenerateTokenAsync(null, cancellationToken);

        await userSessionRepository.AddAsync(new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FamilyId = Guid.NewGuid(),
            TokenHash = refreshToken.TokenHash,
            CreatedAt = now,
            ExpiresAt = refreshToken.ExpiresAt,
            FamilyExpiresAt = refreshToken.FamilyExpiresAt,
            SessionVersion = user.SessionVersion
        }, cancellationToken);

        // another password change won the race, its session stays
        if (!await userRepository.TrySavePasswordChangeAsync(user, expectedVersion, now, cancellationToken))
        {
            throw new ConflictException(AccountMessages.PasswordChangedConcurrently);
        }

        await RemoveUserCardAsync(user.Id, cancellationToken);

        return new AccessInfoModel
        {
            AccessToken = accessToken.Token,
            AccessTokenExpireTime = accessToken.ExpiresInMinutes,
            RefreshToken = refreshToken.Token
        };
    }

    // the admin's user card shows status and lock
    private async Task RemoveUserCardAsync(Guid userId, CancellationToken cancellationToken)
    {
        foreach (var language in SupportedLanguages.All)
        {
            await cache.RemoveAsync(CacheKeys.Admin.UserById(userId, language), cancellationToken);
        }
    }
}
