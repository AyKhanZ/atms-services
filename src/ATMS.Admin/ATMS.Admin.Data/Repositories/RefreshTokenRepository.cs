using System.Linq.Expressions;
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class RefreshTokenRepository(AdminDbContext context) : IRefreshTokenRepository
{
    public Task ClearListAsync(Expression<Func<RevokedToken, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return context.RevokedTokens
            .Where(predicate)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task AddToListAsync(RevokedToken revokedToken, CancellationToken cancellationToken = default)
    {
        await context.RevokedTokens.AddAsync(revokedToken, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return context.RevokedTokens.AnyAsync(t => t.RefreshToken  == refreshToken, cancellationToken);
    }
}