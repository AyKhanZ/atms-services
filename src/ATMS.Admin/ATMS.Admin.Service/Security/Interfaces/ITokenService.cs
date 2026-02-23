using ATMS.Admin.Data.Entities;
using ATMS.Admin.Service.Security.Models;

namespace ATMS.Admin.Service.Security.Interfaces;

public interface ITokenService
{
    string GenerateRefreshToken(User user);
    string GenerateResetPasswordToken(User user);
    Task<TokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken);

    //string GenerateEmailConfirmationToken(User user);
    
    //string DecodeToken(string token);
}
