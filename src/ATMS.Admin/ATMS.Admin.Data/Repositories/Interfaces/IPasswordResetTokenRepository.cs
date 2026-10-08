using System.Linq.Expressions;
using ATMS.Admin.Data.Entities.Tokens;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IPasswordResetTokenRepository
{
    void StageConsume(PasswordResetToken passwordResetToken);

    Task ClearListAsync(Expression<Func<PasswordResetToken, bool>> predicate, CancellationToken cancellationToken);

    Task AddToListAsync(PasswordResetToken passwordResetToken, CancellationToken cancellationToken);

    Task<bool> IsTokenHashExistsAsync(string tokenHash, CancellationToken cancellationToken);

    Task<PasswordResetToken?> FindAsync(Expression<Func<PasswordResetToken, bool>> predicate, CancellationToken cancellationToken);

    Task DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken);
}
