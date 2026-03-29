using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Auth;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public class LoginHandler(
    IUserRepository userRepository,
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

        VerifyPasswords(user, command);

        var accessTokenResult = await accessTokenService.GenerateTokenAsync(user, cancellationToken);
        var refreshToken = await refreshTokenService.GenerateTokenAsync(user, cancellationToken);

        await userRepository.SaveAsync(cancellationToken);

        return new AccessInfoModel
        {
            AccessToken = accessTokenResult.Token,
            AccessTokenExpireTime = accessTokenResult.ExpiresInMinutes,
            RefreshToken = refreshToken
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
            case (int)UserStatus.Inactive:
                throw new AuthException(AuthErrorType.AccountInactive,
                    AuthMessages.AccountInactive);
            case (int)UserStatus.Locked when
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

    private void VerifyPasswords(User user, LoginCommand command)
    {
        var match = passwordHasherService.Verify(command.Password, user.PasswordHash);
        if (match)
        {
            user.FailedLoginCount = 0;
            user.LockoutEnd = null;
            if (user.UserStatusId == (int)UserStatus.Locked)
            {
                user.UserStatusId = (int)UserStatus.Active;
            }
            
            return;
        }

        user.FailedLoginCount++;
        if (user.FailedLoginCount >= 5 && user.UserStatusId == (int)UserStatus.Active)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            user.UserStatusId = (int)UserStatus.Locked;
            user.FailedLoginCount = 0;
        }

        throw new AuthException(AuthErrorType.InvalidCredentials,
            AuthMessages.InvalidLoginCredentials);
    }
}
