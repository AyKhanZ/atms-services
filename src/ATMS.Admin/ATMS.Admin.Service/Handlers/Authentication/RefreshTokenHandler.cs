using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Security.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public class RefreshTokenHandler(
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    IUserRepository userRepository,
    IBlackListService blackListService) : IRequestHandler<RefreshTokenCommand, AccessInfoModel>
{
    public async Task<AccessInfoModel> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        if (await blackListService.IsRefreshTokenRevokedAsync(command.RefreshToken, cancellationToken))
        {
            throw new AuthException(AuthErrorType.InvalidRefreshToken, "Refresh token is revoked.");
        }
        
        var user = await userRepository.FindAsync(u => u.RefreshToken == command.RefreshToken, cancellationToken);

        var newAccessToken = await accessTokenService.GenerateTokenAsync(user, cancellationToken);
        var newRefreshToken = await refreshTokenService.GenerateTokenAsync(user, cancellationToken);

        await userRepository.SaveAsync(cancellationToken);

        return new AccessInfoModel()
        {
            AccessToken = newAccessToken.Token,
            AccessTokenExpireTime = newAccessToken.ExpiresInMinutes,
            RefreshToken = newRefreshToken
        };
    }
}
