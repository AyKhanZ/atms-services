using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;

namespace ATMS.Admin.Service.Security;

public class BlackListService(IRefreshTokenRepository refreshTokenRepository) : IBlackListService
{
    public Task AddToListAsync(Guid userId, string refreshToken, DateTime expiresAt, CancellationToken cancellationToken)
    {
        var revokedToken = new RefreshRevokedToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = refreshToken,
            ExpiresAt = expiresAt
        };
        return refreshTokenRepository
            .AddToListAsync(revokedToken, cancellationToken);
    }
    
    public Task<bool> IsRefreshTokenRevokedAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return refreshTokenRepository
            .IsExistAsync(refreshToken, cancellationToken);
    }

    public Task ClearListAsync(CancellationToken cancellationToken)
    {
        return refreshTokenRepository
            .ClearListAsync(t => t.ExpiresAt < DateTime.UtcNow.AddMonths(-1), cancellationToken);
    }
}
