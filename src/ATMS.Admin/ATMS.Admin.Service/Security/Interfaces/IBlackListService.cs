namespace ATMS.Admin.Service.Security.Interfaces;

public interface IBlackListService
{
    Task AddToListAsync(Guid userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken);
    
    Task<bool> IsRefreshTokenRevokedAsync(string refreshToken, CancellationToken cancellationToken);
    
    Task ClearListAsync(CancellationToken cancellationToken);
}