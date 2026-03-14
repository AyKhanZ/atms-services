using System.Linq.Expressions;
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class RefreshTokenRepository(AdminDbContext context) : IRefreshTokenRepository
{
    public Task ClearListAsync(Expression<Func<RefreshRevokedToken, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return context.RefreshRevokedTokens
            .Where(predicate)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task AddToListAsync(RefreshRevokedToken refreshRevokedToken, CancellationToken cancellationToken = default)
    {
        await context.RefreshRevokedTokens.AddAsync(refreshRevokedToken, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return context.RefreshRevokedTokens.AnyAsync(t => t.Token  == refreshToken, cancellationToken);
    }
}