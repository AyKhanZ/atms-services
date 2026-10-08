using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public sealed class LogoutHandler(
    IUserSessionRepository userSessionRepository,
    IUniqueTokenService uniqueTokenService) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = uniqueTokenService.Hash(command.RefreshToken);
        var session = await userSessionRepository.FindByTokenHashAsync(tokenHash, cancellationToken);

        if (session is null || session.RevokedAt.HasValue)
        {
            return;
        }

        await userSessionRepository.RevokeAsync(session, DateTime.UtcNow, cancellationToken);
    }
}
