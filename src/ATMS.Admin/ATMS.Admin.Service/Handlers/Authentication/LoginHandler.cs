using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Data.Enums;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public class LoginHandler(
    IUserRepository userRepository,
    IUserSessionRepository userSessionRepository,
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    IPasswordHasherService passwordHasherService) : IRequestHandler<LoginCommand, AccessInfoModel>
{
    public async Task<AccessInfoModel> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            throw new AuthException(AuthErrorType.InvalidCredentials,
                AuthMessages.InvalidLoginCredentials);
        }

        EnsureEmailConfirmed(user);

        EnsureAccountIsActive(user);

        if (user.UserStatusId == (int)UserStatusEnum.Locked &&
            user.LockoutEnd <= DateTime.UtcNow)
        {
            user.UserStatusId = (int)UserStatusEnum.Active;
            user.LockoutEnd = null;
        }

        await VerifyPasswordsAsync(user, command, cancellationToken);

        var accessTokenResult = await accessTokenService.GenerateTokenAsync(user, cancellationToken);
        var refreshToken = await refreshTokenService.GenerateTokenAsync(null, cancellationToken);

        await userSessionRepository.AddAsync(
            new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FamilyId = Guid.NewGuid(),
                SessionVersion = user.SessionVersion,
                TokenHash = refreshToken.TokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = refreshToken.ExpiresAt,
                FamilyExpiresAt = refreshToken.FamilyExpiresAt
            },
            cancellationToken);

        await userRepository.SaveAsync(cancellationToken);

        return new AccessInfoModel
        {
            AccessToken = accessTokenResult.Token,
            AccessTokenExpireTime = accessTokenResult.ExpiresInMinutes,
            RefreshToken = refreshToken.Token
        };
    }

    private void EnsureEmailConfirmed(User user)
    {
        if (!user.EmailConfirmed)
        {
            throw new AuthException(AuthErrorType.EmailNotConfirmed,
                AuthMessages.EmailNotConfirmed);
        }
    }

    private void EnsureAccountIsActive(User user)
    {
        switch (user.UserStatusId)
        {
            case (int)UserStatusEnum.Inactive:
                throw new AuthException(AuthErrorType.AccountInactive,
                    AuthMessages.AccountInactive);
            // Locked with no end date was set by an administrator, not by wrong passwords: it lasts
            // until they lift it, and a correct password does not.
            case (int)UserStatusEnum.Locked when !user.LockoutEnd.HasValue:
                throw new AuthException(AuthErrorType.AccountLocked,
                    AuthMessages.AccountLockedByAdministrator);
            case (int)UserStatusEnum.Locked when
                user.LockoutEnd.HasValue &&
                user.LockoutEnd > DateTime.UtcNow:
            {
                var remaining = user.LockoutEnd.Value - DateTime.UtcNow;
                var remainingMinutes = Math.Ceiling(remaining.TotalMinutes);

                throw new AuthException(AuthErrorType.AccountLocked,
                    string.Format(AuthMessages.AccountLocked, remainingMinutes));
            }
        }
    }

    private async Task VerifyPasswordsAsync(User user, LoginCommand command, CancellationToken cancellationToken)
    {
        var match = passwordHasherService.Verify(command.Password, user.PasswordHash);
        if (match)
        {
            user.FailedLoginCount = 0;
            user.LockoutEnd = null;
            if (user.UserStatusId == (int)UserStatusEnum.Locked)
            {
                user.UserStatusId = (int)UserStatusEnum.Active;
            }
            user.LastLogin = DateTime.UtcNow;
            
            return;
        }

        // Saves the lifted expired lockout, if any; the count itself is updated in the database in
        // one statement, so parallel wrong passwords cannot all read the same number.
        await userRepository.SaveAsync(cancellationToken);

        var now = DateTime.UtcNow;
        await userRepository.RegisterFailedPasswordAsync(
            user.Id,
            5,
            now,
            now.AddMinutes(15),
            cancellationToken);

        throw new AuthException(AuthErrorType.InvalidCredentials,
            AuthMessages.InvalidLoginCredentials);
    }
}
