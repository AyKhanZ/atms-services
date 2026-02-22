using ATMS.Admin.Data.Entities;

namespace ATMS.Admin.Service.Security.Interfaces;

public interface ITokenService
{
    string GenerateRefreshToken();
    string GenerateResetPasswordToken();
    Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken);

    //string GenerateEmailConfirmationToken(User user);
    
    //string DecodeToken(string token);
}
