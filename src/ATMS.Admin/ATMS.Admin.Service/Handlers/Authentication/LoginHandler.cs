using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Security.Interfaces;
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

        await VerifyPasswordsAsync(user, command, cancellationToken);

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

    private async Task VerifyPasswordsAsync(User user, LoginCommand command, CancellationToken cancellationToken)
    {
        var match = passwordHasherService.Verify(command.Password, user.PasswordHash);
        if (match)
        {
            user.FailedLoginCount = 0;
            user.LockoutEnd = null; // error
            await userRepository.SaveAsync(cancellationToken);
            return;
        }

        user.FailedLoginCount++;
        if (user.FailedLoginCount == 5 && user.UserStatusId == (int)UserStatusEnum.Active)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            user.UserStatusId = (int)UserStatusEnum.Locked;
            user.FailedLoginCount = 0;
        }
        await userRepository.SaveAsync(cancellationToken);

        throw new AuthException(AuthErrorType.PasswordMismatch, "Incorrect password .");
    }
}
