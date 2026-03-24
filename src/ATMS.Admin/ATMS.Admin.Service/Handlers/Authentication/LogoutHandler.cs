using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Security.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public class LogoutHandler(
    IUserRepository userRepository,
    IBlackListService blackListService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        if (await blackListService.IsRefreshTokenRevokedAsync(command.RefreshToken, cancellationToken))
        {
            throw new AuthException(AuthErrorType.InvalidToken, "Refresh token is revoked .");
        }
        var user = await userRepository.GetAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            throw new AuthException(AuthErrorType.InvalidToken, "User not found .");
        }
        
        await blackListService.AddToListAsync(user, cancellationToken);
    }
}
