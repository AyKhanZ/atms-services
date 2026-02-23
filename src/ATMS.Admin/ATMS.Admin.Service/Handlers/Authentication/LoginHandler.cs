using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public class LoginHandler(
    IUserRepository userRepository,
    ITokenService tokenService) : IRequestHandler<LoginCommand, AccessInfoModel>
{
    public async Task<AccessInfoModel> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByEmail(command.Email, cancellationToken);

        var accessTokenResult = await tokenService.GenerateTokenAsync(user, cancellationToken);
        var refreshToken = tokenService.GenerateRefreshToken(user);

        await userRepository.SaveAsync(cancellationToken);

        return new AccessInfoModel
        {
            AccessToken = accessTokenResult.AccessToken,
            AccessTokenExpireTime = accessTokenResult.ExpiresInMinutes,
            RefreshToken = refreshToken
        };
    }
}
