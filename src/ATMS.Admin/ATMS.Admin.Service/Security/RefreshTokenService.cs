using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Security;

public class RefreshTokenService(
    IUserRepository userRepository,
    IUniqueTokenService uniqueTokenService,
    IConfiguration configuration) : IRefreshTokenService
{
    private readonly JwtOptions _jwtOptions =
        configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.JwtSectionNotFound,
                string.Format(LogMessages.ConfigSectionNotFound, nameof(JwtOptions)));

    
    public async Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken)
    {
        var refreshToken = await uniqueTokenService.GenerateUniqueAsync(
            async token => await userRepository.IsExistAsync(u => u.RefreshToken == token, cancellationToken)
        );

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays);

        return refreshToken;
    }
}
