using System.IdentityModel.Tokens.Jwt;
using ATMS.Application.Constants;
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

    public Guid RoleId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User
                .FindFirst(CustomClaimTypes.RoleId)?.Value;

            if (claim is null || !Guid.TryParse(claim, out var roleId))
            {
                throw new AuthException(AuthErrorType.InvalidCredentials, ExceptionMessages.InvalidCredentials);
            }
            return roleId;
        }
    }
    
    public string UserType
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User
                .FindFirst(CustomClaimTypes.UserType)?.Value;

            return claim ?? throw new AuthException(AuthErrorType.InvalidCredentials, ExceptionMessages.InvalidCredentials);
        }
    }

    
    public bool HasCompletedSurvey
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User
                .FindFirst(CustomClaimTypes.HasCompletedSurvey)?.Value;

            if (claim is null || !bool.TryParse(claim, out var hasCompletedSurvey))
            {
                throw new AuthException(AuthErrorType.InvalidCredentials, ExceptionMessages.InvalidCredentials);
            }

            return hasCompletedSurvey;
        }
    }
    
    public bool EmailConfirmed
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User
                .FindFirst(CustomClaimTypes.EmailConfirmed)?.Value;

            if (claim is null || !bool.TryParse(claim, out var emailConfirmed))
            {
                throw new AuthException(AuthErrorType.InvalidCredentials, ExceptionMessages.InvalidCredentials);
            }

            return emailConfirmed;
        }
    }
}