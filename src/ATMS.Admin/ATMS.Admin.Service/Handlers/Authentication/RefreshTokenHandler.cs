using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Enums;
using ATMS.Data.Enums;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Authentication;

public sealed class RefreshTokenHandler(
    IAccessTokenService accessTokenService,
    IRefreshTokenService refreshTokenService,
    IUniqueTokenService uniqueTokenService,
    IUserSessionRepository userSessionRepository) : IRequestHandler<RefreshTokenCommand, AccessInfoModel>
{
    public async Task<AccessInfoModel> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var tokenHash = uniqueTokenService.Hash(command.RefreshToken);
        var session = await userSessionRepository.FindByTokenHashAsync(tokenHash, cancellationToken);

        if (session is null)
        {
            throw new AuthException(AuthErrorTypeEnum.InvalidToken, AuthMessages.InvalidToken);
        }

        if (session.RevokedAt.HasValue)
        {
            await userSessionRepository.RevokeFamilyAsync(session.FamilyId, now, cancellationToken);
            throw new AuthException(AuthErrorTypeEnum.InvalidToken, AuthMessages.InvalidToken);
        }

        if (session.ExpiresAt <= now || session.FamilyExpiresAt <= now)
        {
            await userSessionRepository.RevokeFamilyAsync(session.FamilyId, now, cancellationToken);
            throw new AuthException(AuthErrorTypeEnum.InvalidToken, AuthMessages.InvalidToken);
        }

        // a timed lock only blocks a new sign-in, the session that is already open stays
        if (session.User.UserStatusId == (int)UserStatusEnum.Inactive)
        {
            await userSessionRepository.RevokeFamilyAsync(session.FamilyId, now, cancellationToken);
            throw new AuthException(AuthErrorTypeEnum.AccountInactive, AuthMessages.AccountInactive);
        }

        // password changed after this session started
        if (session.SessionVersion != session.User.SessionVersion)
        {
            await userSessionRepository.RevokeFamilyAsync(session.FamilyId, now, cancellationToken);
            throw new AuthException(AuthErrorTypeEnum.InvalidToken, AuthMessages.InvalidToken);
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
            FamilyExpiresAt = session.FamilyExpiresAt,
            SessionVersion = session.SessionVersion
        };

        if (!await userSessionRepository.RotateAsync(session, replacement, now, cancellationToken))
        {
            await userSessionRepository.RevokeFamilyAsync(session.FamilyId, now, cancellationToken);
            throw new AuthException(AuthErrorTypeEnum.InvalidToken, AuthMessages.InvalidToken);
        }

        return new AccessInfoModel
        {
            AccessToken = accessToken.Token,
            AccessTokenExpireTime = accessToken.ExpiresInMinutes,
            RefreshToken = refreshToken.Token
        };
    }
}
