using System.Linq.Expressions;
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class PasswordResetTokenRepository(AdminDbContext context) : IPasswordResetTokenRepository
{
    public Task ClearListAsync(
        Expression<Func<PasswordResetToken, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return context.PasswordResetTokens
            .Where(predicate)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task AddToListAsync(
        PasswordResetToken passwordResetToken,
        CancellationToken cancellationToken)
    {
        await context.PasswordResetTokens.AddAsync(passwordResetToken, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(
        string passwordResetToken,
        CancellationToken cancellationToken)
    {
        return context.PasswordResetTokens.AnyAsync(t => t.Token  == passwordResetToken, cancellationToken);
    }

    public Task<PasswordResetToken?> FindAsync(
        Expression<Func<PasswordResetToken, bool>> predicate,
        CancellationToken cancellationToken)
    {
        return context.PasswordResetTokens.FirstOrDefaultAsync(predicate, cancellationToken);
    }
}
