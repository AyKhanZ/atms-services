using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class UserSessionRepository(AdminDbContext context) : IUserSessionRepository
{
    public Task<UserSession?> FindByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return context.UserSessions
            .Include(session => session.User)
            .FirstOrDefaultAsync(session => session.TokenHash == tokenHash, cancellationToken);
    }

    public Task<bool> IsTokenHashExistsAsync(string tokenHash, CancellationToken cancellationToken)
    {
        return context.UserSessions
            .AnyAsync(session => session.TokenHash == tokenHash, cancellationToken);
    }

    public async Task AddAsync(UserSession session, CancellationToken cancellationToken)
    {
        await context.UserSessions.AddAsync(session, cancellationToken);
    }

    public async Task<bool> RotateAsync(
        UserSession currentSession,
        UserSession replacementSession,
        DateTime revokedAt,
        CancellationToken cancellationToken)
    {
        currentSession.RevokedAt = revokedAt;
        await context.UserSessions.AddAsync(replacementSession, cancellationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            context.ChangeTracker.Clear();
            return false;
        }
    }

    public async Task RevokeAsync(
        UserSession session,
        DateTime revokedAt,
        CancellationToken cancellationToken)
    {
        if (session.RevokedAt.HasValue)
        {
            return;
        }

        session.RevokedAt = revokedAt;

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            context.ChangeTracker.Clear();
        }
    }

    public async Task RevokeFamilyAsync(
        Guid familyId,
        DateTime revokedAt,
        CancellationToken cancellationToken)
    {
        await context.UserSessions
            .Where(session => session.FamilyId == familyId && session.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(session => session.RevokedAt, revokedAt),
                cancellationToken);
    }

    public async Task RevokeAllAsync(
        Guid userId,
        DateTime revokedAt,
        CancellationToken cancellationToken)
    {
        await context.UserSessions
            .Where(session => session.UserId == userId && session.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(session => session.RevokedAt, revokedAt),
                cancellationToken);
    }

    public async Task DeleteExpiredAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        await context.UserSessions
            .Where(session => session.FamilyExpiresAt < utcNow)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
