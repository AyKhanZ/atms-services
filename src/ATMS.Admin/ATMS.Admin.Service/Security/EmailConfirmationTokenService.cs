using System.Security.Claims;
using System.Text;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Security.Constants;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Admin.Service.Security.Models;
using ATMS.Exceptions.Configuration;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ATMS.Admin.Service.Security;

public class EmailConfirmationTokenService(IConfiguration configuration) : IEmailConfirmationTokenService
{
    
    private readonly JwtOptions _jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>()
                                              ?? throw new ConfigurationException(ConfigurationErrorType.JwtSectionNotFound,
                                                  $"Configuration for section '{nameof(JwtOptions)}' is not found or could not be loaded.");

    public EmailConfirmationTokenResult GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(CustomClaimTypes.EmailConfirmed, user.EmailConfirmed.ToString().ToLower()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(_jwtOptions.EmailConfirmationTokenExpirationInHours),
            SigningCredentials = credentials,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience
        };

        var handler = new JsonWebTokenHandler();

        var token = handler.CreateToken(tokenDescriptor);
        var tokenValidity = DateTime.UtcNow.AddHours(_jwtOptions.EmailConfirmationTokenExpirationInHours);
        
        return new EmailConfirmationTokenResult(token, tokenValidity);
    }
    
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token)
    {
        var handler = new JsonWebTokenHandler();

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = _jwtOptions.Audience,
            
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var result = await handler.ValidateTokenAsync(token, parameters);

        if (!result.IsValid)
            return null;

        return new ClaimsPrincipal(result.ClaimsIdentity);
    }
}
