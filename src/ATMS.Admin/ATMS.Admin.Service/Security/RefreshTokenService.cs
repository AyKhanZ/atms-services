using System.Security.Cryptography;
using System.Text;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Security;

public class RefreshTokenService(
    IUserSessionRepository userSessionRepository,
    IUniqueTokenService uniqueTokenService,
    IConfiguration configuration) : IRefreshTokenService
{
    private readonly JwtOptions _jwtOptions =
        configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.JwtSectionNotFound,
                string.Format(LogMessages.ConfigSectionNotFound, nameof(JwtOptions)));

    public async Task<RefreshTokenResult> GenerateTokenAsync(DateTime? familyExpiresAt, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var absoluteExpiration = familyExpiresAt
            ?? now.AddDays(_jwtOptions.MaxRefreshTokenLifetimeExpirationInDays);

        var refreshToken = await uniqueTokenService.GenerateUniqueAsync(
            token => userSessionRepository.IsTokenHashExistsAsync(HashToken(token), cancellationToken));

        var expiresAt = now.AddDays(_jwtOptions.RefreshTokenExpirationInDays);
        if (expiresAt > absoluteExpiration)
        {
            expiresAt = absoluteExpiration;
        }

        return new RefreshTokenResult(
            refreshToken,
            HashToken(refreshToken),
            expiresAt,
            absoluteExpiration);
    }

    public string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return WebEncoders.Base64UrlEncode(hash);
    }
}
