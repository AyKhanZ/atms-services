using System.Security.Cryptography;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Security;

public class RefreshTokenService(
    IUserRepository userRepository,
    IConfiguration configuration) : IRefreshTokenService
{
    private readonly JwtOptions _jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
                                           ?? throw new ConfigurationException(ConfigurationErrorType.JwtSectionNotFound,
                                               $"Configuration for section '{nameof(JwtOptions)}' is not found or could not be loaded.");

    private static string GenerateSecureToken(int size = 32) =>
        WebEncoders.Base64UrlEncode(
            RandomNumberGenerator.GetBytes(size));

    private async Task<string> GenerateUniqueTokenAsync(Func<string, Task<bool>> isTokenExistAsync, int maxAttempts = 5)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var token = GenerateSecureToken();
            if (!await isTokenExistAsync(token))
            {
                return token;
            }
        }

        throw new AuthException(AuthErrorType.InvalidRefreshToken,
            "Failed to generate a unique token after several attempts.");
    }
    
    public async Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken)
    {
        var refreshToken = await GenerateUniqueTokenAsync(
            async token => await userRepository.IsExistAsync(u => u.RefreshToken == token, cancellationToken)
        );

        user.RefreshToken = refreshToken;
        //user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(10);
        user.RefreshTokenCreatedAt = DateTime.UtcNow;

        return refreshToken;
    }
}