using System.IdentityModel.Tokens.Jwt;
using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Application.Exceptions.Resources;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[ApiController]
public abstract class AdminControllerBase : ControllerBase
{
    protected Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new AuthException(AuthErrorType.InvalidCredentials, ExceptionMessages.InvalidCredentials);
        }
        
        return userId;
    }
}
