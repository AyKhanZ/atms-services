using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Admin.Service.Security.Models;
using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using ATMS.Admin.Data.Repositories.Interfaces;

namespace ATMS.Admin.Service.Security;

public class AccessTokenService(
    IUserRepository userRepository,
    IConfiguration configuration
    ) : IAccessTokenService
{
    private readonly JwtOptions _jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
                                             ?? throw new ConfigurationException(ConfigurationErrorType.JwtSectionNotFound,
                                                 $"Configuration for section '{nameof(JwtOptions)}' is not found or could not be loaded.");

    public async Task<AccessTokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken)
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

        return new AccessTokenResult(token, tokenValidity);
    }
}
