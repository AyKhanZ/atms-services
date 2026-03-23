using System.IdentityModel.Tokens.Jwt;
using ATMS.Admin.Service.Exceptions.Auth;
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
            throw new AuthException(AuthErrorType.InvalidCredentials,
                "User ID is invalid or not found in claims.");
        }
        
        return userId;
    }
}
