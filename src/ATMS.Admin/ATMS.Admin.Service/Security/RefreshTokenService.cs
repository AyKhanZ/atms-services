using Microsoft.Extensions.Options;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;

namespace ATMS.Admin.Service.Security;

public sealed class RefreshTokenService(
    IUserSessionRepository userSessionRepository,
    IUniqueTokenService uniqueTokenService,
    IOptions<JwtOptions> jwtOptions) : IRefreshTokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<RefreshTokenResult> GenerateTokenAsync(DateTime? familyExpiresAt, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var absoluteExpiration = familyExpiresAt
            ?? now.AddDays(_jwtOptions.MaxRefreshTokenLifetimeExpirationInDays);

        var refreshToken = await uniqueTokenService.GenerateUniqueAsync(
            token => userSessionRepository.IsTokenHashExistsAsync(uniqueTokenService.Hash(token), cancellationToken));

        var expiresAt = now.AddDays(_jwtOptions.RefreshTokenExpirationInDays);
        if (expiresAt > absoluteExpiration)
        {
            expiresAt = absoluteExpiration;
        }

        return new RefreshTokenResult(
            refreshToken,
            uniqueTokenService.Hash(refreshToken),
            expiresAt,
            absoluteExpiration);
    }
}
