using System.Linq.Expressions;
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

    public async Task<bool> TryAddToListAsync(RefreshRevokedToken refreshRevokedToken,
        CancellationToken cancellationToken = default)
    {
        await context.RefreshRevokedTokens.AddAsync(refreshRevokedToken, cancellationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_RefreshRevokedTokens_Token"
            })
        {
            return false;
        }
    }

    public Task<bool> IsExistAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return context.RefreshRevokedTokens.AnyAsync(t => t.Token  == refreshToken, cancellationToken);
    }
}
