using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Security.Models;

namespace ATMS.Admin.Service.Security.Interfaces;

public interface ITokenService
{
    Task<string> GenerateRefreshToken(User user, CancellationToken cancellationToken);
    string GenerateResetPasswordToken(User user);
    Task<AccessTokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken);

    //string GenerateEmailConfirmationToken(User user);
    
    //string DecodeToken(string token);
}
