using System.Linq.Expressions;
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public sealed class PasswordResetTokenRepository(AdminDbContext context) : IPasswordResetTokenRepository
{
    public void StageConsume(PasswordResetToken passwordResetToken)
    {
        // if a parallel reset already deleted this link, SaveChanges fails and the whole change rolls back
        context.PasswordResetTokens.Remove(passwordResetToken);
    }

    public async Task ClearListAsync(
        Expression<Func<PasswordResetToken, bool>> predicate,
        CancellationToken cancellationToken)
    {
        var tokens = await context.PasswordResetTokens
            .Where(predicate)
            .ToListAsync(cancellationToken);

        context.PasswordResetTokens.RemoveRange(tokens);
    }

    public async Task AddToListAsync(
        PasswordResetToken passwordResetToken,
        CancellationToken cancellationToken)
    {
        await context.PasswordResetTokens.AddAsync(passwordResetToken, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsTokenHashExistsAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        return context.PasswordResetTokens.AnyAsync(t => t.TokenHash == tokenHash, cancellationToken);
    }

    public Task<PasswordResetToken?> FindAsync(
        Expression<Func<PasswordResetToken, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return context.PasswordResetTokens.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        await context.PasswordResetTokens
            .Where(token => token.ExpiresAt < utcNow)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
