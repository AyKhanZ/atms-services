using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Auth;
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
        var user = await userRepository.FindAsync(u => u.RefreshToken == command.RefreshToken, cancellationToken);
        
        if (user?.RefreshToken is null
            || user.RefreshTokenExpiresAt is null
            || user.RefreshTokenExpiresAt <= DateTime.UtcNow
            || await blackListService.IsRefreshTokenRevokedAsync(command.RefreshToken, cancellationToken))
        {
            throw new AuthException(AuthErrorType.InvalidToken, AuthMessages.InvalidToken);
        }

        var oldRefreshToken = user.RefreshToken;
        var oldExpiresAt = user.RefreshTokenExpiresAt.Value;
        
        var newAccessToken = await accessTokenService.GenerateTokenAsync(user, cancellationToken);
        var newRefreshToken = await refreshTokenService.GenerateTokenAsync(user, cancellationToken);

        await blackListService.AddToListAsync(user.Id, oldRefreshToken, oldExpiresAt, cancellationToken);
        await userRepository.SaveAsync(cancellationToken);

        return new AccessInfoModel
        {
            AccessToken = newAccessToken.Token,
            AccessTokenExpireTime = newAccessToken.ExpiresInMinutes,
            RefreshToken = newRefreshToken
        };
    }
}
