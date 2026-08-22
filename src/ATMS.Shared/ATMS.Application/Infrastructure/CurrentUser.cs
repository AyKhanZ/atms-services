using System.IdentityModel.Tokens.Jwt;
using ATMS.Application.Constants;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Data.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ATMS.Application.Infrastructure;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser, IAuditActorAccessor
{
    Guid? IAuditActorAccessor.UserId => TryGetUserId();

    public Guid Id
    {
        get
        {
            var id = TryGetUserId();
            if (!id.HasValue)
            {
                throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);
            }
            return id.Value;
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
                throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);
            }
            return roleId;
        }
    }
    
    public Guid? OrganizationId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User
                .FindFirst(CustomClaimTypes.OrganizationId)?.Value;

            if (claim is null || !Guid.TryParse(claim, out var orgId))
            {
                throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);
            }
            return orgId;
        }
    }
    
    public string UserType
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User
                .FindFirst(CustomClaimTypes.UserType)?.Value;

            return claim ?? throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);
        }
    }

    private Guid? TryGetUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
