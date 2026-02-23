using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Admin.Service.Security.Models;
using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ATMS.Admin.Service.Security;

public class TokenService(
    IUserRepository userRepository,
    IConfiguration configuration
    ) : ITokenService
{
    private readonly JwtOptions _jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
                                             ?? throw new ConfigurationException(ConfigurationErrorType.JwtSectionNotFound,
                                                 $"Configuration for section '{nameof(JwtOptions)}' is not found or could not be loaded.");

    private static string GenerateSecureToken(int size = 32) =>
        WebEncoders.Base64UrlEncode(
            RandomNumberGenerator.GetBytes(size));

    public string GenerateRefreshToken(User user)
    {
        user.RefreshToken = GenerateSecureToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays);
        user.RefreshTokenCreatedAt = DateTime.UtcNow;

        return GenerateSecureToken();
    }

    public string GenerateResetPasswordToken(User user) => GenerateSecureToken();

    public async Task<TokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var roles = await userRepository.GetRolesAsync(user.Id, cancellationToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(CustomClaimTypes.Surname, user.Surname),
            new(CustomClaimTypes.HasCompletedSurvey, user.HasCompletedSurvey.ToString().ToLower()),
            new(CustomClaimTypes.EmailConfirmed, user.EmailConfirmed.ToString().ToLower()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.Name)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.TokenExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience
        };

        var handler = new JsonWebTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);
        var tokenValidity = DateTime.UtcNow.AddMinutes(_jwtOptions.TokenExpirationInMinutes);

        return new TokenResult(token, tokenValidity);
    }
}
