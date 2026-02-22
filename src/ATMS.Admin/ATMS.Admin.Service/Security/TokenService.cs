using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Infrastructure.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ATMS.Admin.Service.Security;

public class TokenService(
    IUserRepository userRepository,
    JwtOptions jwtOptions
    ) : ITokenService
{
    private static string GenerateSecureToken(int size = 32) =>
        WebEncoders.Base64UrlEncode(
            RandomNumberGenerator.GetBytes(size));
    public string GenerateRefreshToken() => GenerateSecureToken();

    public string GenerateResetPasswordToken() => GenerateSecureToken();

    public async Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));
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
            Expires = DateTime.UtcNow.AddMinutes(jwtOptions.TokenExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = jwtOptions.Issuer,
            Audience = jwtOptions.Audience
        };

        var handler = new JsonWebTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);

        return token;
    }
}
