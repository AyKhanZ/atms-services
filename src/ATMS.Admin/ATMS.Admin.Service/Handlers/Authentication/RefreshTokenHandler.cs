using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public class RefreshTokenHandler(
    ITokenService tokenService,
    IUserRepository userRepository
    ) : IRequestHandler<RefreshTokenCommand, AccessInfoModel>
{
    public async Task<AccessInfoModel> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(u => u.RefreshToken == command.RefreshToken, cancellationToken);

        var newAccessToken = await tokenService.GenerateTokenAsync(user, cancellationToken);
        var newRefreshToken = await tokenService.GenerateRefreshToken(user, cancellationToken);

        await userRepository.SaveAsync(cancellationToken);

        return new AccessInfoModel()
        {
            AccessToken = newAccessToken.Token,
            AccessTokenExpireTime = newAccessToken.ExpiresInMinutes,
            RefreshToken = newRefreshToken
        };
    }
}
