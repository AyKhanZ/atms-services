using ATMS.Admin.Data.Entities;

namespace ATMS.Admin.Service.Security.Interfaces;

public interface IBlackListService
{
    Task AddToListAsync(User user, CancellationToken token);
    
    Task<bool> IsRefreshTokenRevokedAsync(string refreshToken, CancellationToken cancellationToken);
    
    Task ClearListAsync(CancellationToken cancellationToken);
}