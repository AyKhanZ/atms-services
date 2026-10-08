using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Admin.Service.Security.Models;
using ATMS.Application.Constants;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Constants;
using ATMS.Infrastructure.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ATMS.Admin.Service.Security;

public sealed class AccessTokenService(
    IUserRepository userRepository,
    IOptions<JwtOptions> jwtOptions) : IAccessTokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AccessTokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var roles = await userRepository.GetRolesAsync(user.Id, cancellationToken);

        var role = roles.Single();
        var permissions = await userRepository.GetPermissionsAsync(user.Id, cancellationToken);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(CustomClaimTypes.Surname, user.Surname),
            new(CustomClaimTypes.EmailConfirmed, user.EmailConfirmed.ToString().ToLowerInvariant()),
            new(CustomClaimTypes.OnboardingCompleted, user.HasCompletedOnboarding.ToString().ToLowerInvariant()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(CustomClaimTypes.RoleId, role.Id.ToString()),
            new(CustomClaimTypes.UserType, role.Name)
        };

        claims.AddRange(permissions.Select(permission => new Claim(CustomClaimTypes.Permission, permission.Code)));

        if (role.Id != RoleIds.Employee && user.OrganizationId.HasValue)
        {
            claims.Add(new Claim(CustomClaimTypes.OrganizationId, user.OrganizationId.Value.ToString()));
        }

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
