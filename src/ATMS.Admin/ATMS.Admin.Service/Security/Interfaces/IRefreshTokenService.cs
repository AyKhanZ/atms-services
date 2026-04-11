using ATMS.Admin.Data.Entities;

namespace ATMS.Admin.Service.Security.Interfaces;

public interface IRefreshTokenService
{
    Task<string> GenerateTokenAsync(User user, CancellationToken cancellationToken);
}