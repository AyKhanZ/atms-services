using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Resources;
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
            throw new AuthException(AuthErrorType.InvalidToken, AuthMessages.InvalidToken);
        }
        
        var user = await userRepository.FindAsync(u => u.Id == command.UserId, cancellationToken);
        if (user?.RefreshToken is null || user.RefreshTokenExpiresAt is null)
        {
            throw new AuthException(AuthErrorType.InvalidToken, AuthMessages.InvalidToken);
        }
        
        await blackListService.AddToListAsync(user.Id, user.RefreshToken, user.RefreshTokenExpiresAt.Value, cancellationToken);
        
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        await userRepository.SaveAsync(cancellationToken);
    }
}
