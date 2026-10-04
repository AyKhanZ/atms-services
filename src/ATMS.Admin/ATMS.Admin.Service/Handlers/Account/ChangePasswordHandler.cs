using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
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
    IRefreshTokenService refreshTokenService) : IRequestHandler<ChangePasswordCommand, AccessInfoModel>
{
    public async Task<AccessInfoModel> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(u => u.Id == currentUser.Id, cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);

        var now = DateTime.UtcNow;
        if (user.UserStatusId == (int)UserStatusEnum.Locked &&
            user.LockoutEnd.HasValue && user.LockoutEnd > now)
        {
            var remainingMinutes = Math.Ceiling((user.LockoutEnd.Value - now).TotalMinutes);
            throw new AuthException(AuthErrorType.AccountLocked,
                string.Format(AuthMessages.AccountLocked, remainingMinutes));
        }

        if (user.UserStatusId == (int)UserStatusEnum.Inactive)
        {
            throw new AuthException(AuthErrorType.AccountInactive, AuthMessages.AccountInactive);
        }

        if (!passwordHasherService.Verify(command.OldPassword, user.PasswordHash))
        {
            user.FailedLoginCount++;
            var lockedNow = user.FailedLoginCount >= 5;
            if (lockedNow)
            {
                user.LockoutEnd = now.AddMinutes(15);
                user.UserStatusId = (int)UserStatusEnum.Locked;
                user.FailedLoginCount = 0;
            }

            await userRepository.SaveAsync(cancellationToken);

            // The attempt that locks the account says so at once, not on the next try.
            if (lockedNow)
            {
                throw new AuthException(AuthErrorType.AccountLocked,
                    string.Format(AuthMessages.AccountLocked, 15));
            }

            throw new ValidationException([
                new ValidationFailure(nameof(command.OldPassword), AccountMessages.OldPasswordIncorrect)
            ]);
        }

        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        if (user.UserStatusId == (int)UserStatusEnum.Locked)
        {
            user.UserStatusId = (int)UserStatusEnum.Active;
        }

        user.PasswordHash = passwordHasherService.Hash(command.NewPassword);

        var accessToken = await accessTokenService.GenerateTokenAsync(user, cancellationToken);
        var refreshToken = await refreshTokenService.GenerateTokenAsync(null, cancellationToken);

        await userSessionRepository.ReplaceAllAsync(new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            FamilyId = Guid.NewGuid(),
            TokenHash = refreshToken.TokenHash,
            CreatedAt = now,
            ExpiresAt = refreshToken.ExpiresAt,
            FamilyExpiresAt = refreshToken.FamilyExpiresAt
        }, now, cancellationToken);

        await userRepository.SaveAsync(cancellationToken);

        return new AccessInfoModel
        {
            AccessToken = accessToken.Token,
            AccessTokenExpireTime = accessToken.ExpiresInMinutes,
            RefreshToken = refreshToken.Token
        };
    }
}
