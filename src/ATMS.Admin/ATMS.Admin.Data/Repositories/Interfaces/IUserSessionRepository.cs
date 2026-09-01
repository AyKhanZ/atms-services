using ATMS.Admin.Data.Entities.Tokens;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IUserSessionRepository
{
    Task<UserSession?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task<bool> IsTokenHashExistsAsync(string tokenHash, CancellationToken cancellationToken);

    Task AddAsync(UserSession session, CancellationToken cancellationToken);

    Task<bool> RotateAsync(
        UserSession currentSession,
        UserSession replacementSession,
        DateTime revokedAt,
        CancellationToken cancellationToken);

    Task RevokeAsync(UserSession session, DateTime revokedAt, CancellationToken cancellationToken);

    Task RevokeFamilyAsync(Guid familyId, DateTime revokedAt, CancellationToken cancellationToken);

    Task RevokeAllAsync(Guid userId, DateTime revokedAt, CancellationToken cancellationToken);

    Task DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken);
}
