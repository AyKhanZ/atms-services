using System.Linq.Expressions;
using ATMS.Admin.Data.Entities.Tokens;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task ClearListAsync(Expression<Func<RefreshRevokedToken, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddToListAsync(RefreshRevokedToken refreshRevokedToken, CancellationToken cancellationToken = default);
    Task<bool> IsExistAsync(string refreshToken, CancellationToken cancellationToken = default);
}