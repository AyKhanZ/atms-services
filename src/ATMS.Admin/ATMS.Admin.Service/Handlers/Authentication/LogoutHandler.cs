using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public class LogoutHandler(
    IUserSessionRepository userSessionRepository,
    IRefreshTokenService refreshTokenService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenService.HashToken(command.RefreshToken);
        var session = await userSessionRepository.FindByTokenHashAsync(tokenHash, cancellationToken);

        if (session is null || session.RevokedAt.HasValue)
        {
            return;
        }

        await userSessionRepository.RevokeAsync(session, DateTime.UtcNow, cancellationToken);
    }
}
