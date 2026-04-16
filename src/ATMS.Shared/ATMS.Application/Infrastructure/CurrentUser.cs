using System.IdentityModel.Tokens.Jwt;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ATMS.Application.Infrastructure;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid Id
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User
                .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            if (claim is null || !Guid.TryParse(claim, out var id))
            {
                throw new AuthException(AuthErrorType.InvalidCredentials, ExceptionMessages.InvalidCredentials);
            }
            return id;
        }
    }
}
