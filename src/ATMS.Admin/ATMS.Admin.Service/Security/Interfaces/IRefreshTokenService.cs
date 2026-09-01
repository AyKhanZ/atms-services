using ATMS.Admin.Service.Security.Models;

namespace ATMS.Admin.Service.Security.Interfaces;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> GenerateTokenAsync(
        DateTime? familyExpiresAt,
        CancellationToken cancellationToken);

    string HashToken(string token);
}
