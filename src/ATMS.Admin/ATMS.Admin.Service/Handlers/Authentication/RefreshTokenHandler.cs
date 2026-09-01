using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Data.Enums;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public class RefreshTokenHandler(
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    IUserSessionRepository userSessionRepository) : IRequestHandler<RefreshTokenCommand, AccessInfoModel>
{
    public async Task<AccessInfoModel> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var tokenHash = refreshTokenService.HashToken(command.RefreshToken);
        var session = await userSessionRepository.FindByTokenHashAsync(tokenHash, cancellationToken);

        if (session is null)
        {
            throw new AuthException(AuthErrorType.InvalidToken, AuthMessages.InvalidToken);
        }

        if (session.RevokedAt.HasValue)
        {
            await userSessionRepository.RevokeFamilyAsync(session.FamilyId, now, cancellationToken);
            throw new AuthException(AuthErrorType.InvalidToken, AuthMessages.InvalidToken);
        }

        if (session.ExpiresAt <= now || session.FamilyExpiresAt <= now)
        {
            await userSessionRepository.RevokeFamilyAsync(session.FamilyId, now, cancellationToken);
            throw new AuthException(AuthErrorType.InvalidToken, AuthMessages.InvalidToken);
        }

        if (session.User.UserStatusId != (int)UserStatusEnum.Active)
        {
            await userSessionRepository.RevokeFamilyAsync(session.FamilyId, now, cancellationToken);
            throw new AuthException(AuthErrorType.AccountInactive, AuthMessages.AccountInactive);
        }

        var accessToken = await accessTokenService.GenerateTokenAsync(session.User, cancellationToken);
        var refreshToken = await refreshTokenService.GenerateTokenAsync(
            session.FamilyExpiresAt,
            cancellationToken);

        var replacement = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = session.UserId,
            FamilyId = session.FamilyId,
            TokenHash = refreshToken.TokenHash,
            CreatedAt = now,
            ExpiresAt = refreshToken.ExpiresAt,
            FamilyExpiresAt = session.FamilyExpiresAt
        };

        if (!await userSessionRepository.RotateAsync(session, replacement, now, cancellationToken))
        {
            await userSessionRepository.RevokeFamilyAsync(session.FamilyId, now, cancellationToken);
            throw new AuthException(AuthErrorType.InvalidToken, AuthMessages.InvalidToken);
        }

        return new AccessInfoModel
        {
            AccessToken = accessToken.Token,
            AccessTokenExpireTime = accessToken.ExpiresInMinutes,
            RefreshToken = refreshToken.Token
        };
    }
}
