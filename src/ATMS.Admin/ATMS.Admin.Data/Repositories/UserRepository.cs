using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;

namespace ATMS.Admin.Data.Repositories;

public class UserRepository(AdminDbContext context) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await context.Users.AddAsync(user, cancellationToken);
    }

    public async Task CreateAsync(User user, CancellationToken cancellationToken)
    {
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<User>> GetAsync(
        ACriteria<User> filterCriteria,
        PaginationCriteria<User> pagination,
        CancellationToken cancellationToken)
    {
        var query = context.Users
            .Include(u => u.UserStatus).ThenInclude(s => s.Translations)
            .AsNoTracking()
            .AsSplitQuery();
        
        query = filterCriteria.Apply(query);
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var users = await pagination.Apply(query).ToListAsync(cancellationToken);
        
        return new PagedResult<User>
        {
            Items      = users.ToArray(),
            TotalCount = totalCount,
            Page       = pagination.Page,
            PageSize   = pagination.PageSize
        };
    }

    public Task<User?> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
    {
        return context.Users
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<User?> GetMeAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Users
            .AsNoTracking()
            .Include(x => x.Language)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<User>> GetAsync(CancellationToken cancellationToken)
    {
        return context.Users
            .Include(u => u.UserStatus).ThenInclude(s => s.Translations)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
    }

    public Task<User?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Users
            .AsNoTracking()
            .Include(u => u.Gender).ThenInclude(g => g.Translations)
            .Include(u => u.MaritalStatus).ThenInclude(m => m.Translations)
            .Include(u => u.UserStatus).ThenInclude(s => s.Translations)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<Role>> GetRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<Permission>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken)
        => context.Users.AnyAsync(predicate, cancellationToken);


    public async Task<bool> RegisterFailedPasswordAsync(
        Guid userId,
        int maxAttempts,
        DateTime now,
        DateTime lockoutEnd,
        CancellationToken cancellationToken)
    {
        // Read-and-write in one UPDATE: Postgres locks the row, so wrong passwords sent in parallel
        // are counted one after another instead of all reading the same count and writing count + 1.
        // Only an account that can be locked is touched: active, or with its timed lockout over. Once a
        // parallel attempt has locked it, the rest no longer count against the next 15 minutes, and a
        // lock set by an administrator keeps its count. A timed lockout that is over is lifted here as
        // well, so the account reads as active again. SET reads the old row, RETURNING the new one.
        const int active = (int)UserStatusEnum.Active;
        const int locked = (int)UserStatusEnum.Locked;

        var updated = await context.Database
            .SqlQuery<bool>($"""
                UPDATE "Users"
                SET "FailedLoginCount" = CASE
                        WHEN "FailedLoginCount" + 1 >= {maxAttempts} THEN 0
                        ELSE "FailedLoginCount" + 1 END,
                    "LockoutEnd" = CASE
                        WHEN "FailedLoginCount" + 1 >= {maxAttempts} THEN {lockoutEnd}
                        ELSE NULL END,
                    "UserStatusId" = CASE
                        WHEN "FailedLoginCount" + 1 >= {maxAttempts} THEN {locked}
                        ELSE {active} END
                WHERE "Id" = {userId}
                    AND ("UserStatusId" = {active}
                        OR ("UserStatusId" = {locked} AND "LockoutEnd" <= {now}))
                RETURNING "UserStatusId" = {locked} AS "Value"
                """)
            .ToListAsync(cancellationToken);

        // No row: a parallel attempt locked the account first, so it is locked all the same.
        return updated.Count == 0 || updated[0];
    }

    public async Task<bool> TrySavePasswordChangeAsync(
        User user,
        int expectedVersion,
        DateTime revokedAt,
        CancellationToken cancellationToken)
    {
        // One transaction for the whole password change. The new version is written by a conditional
        // UPDATE: a second change that read the same version waits on the row lock, then finds the
        // version moved and matches nothing, so it rolls back instead of issuing a second session under
        // the same version. A plain read-modify-write would let both through.
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var raised = await context.Database.ExecuteSqlAsync($"""
            UPDATE "Users"
            SET "SessionVersion" = {expectedVersion + 1}
            WHERE "Id" = {user.Id} AND "SessionVersion" = {expectedVersion}
            """, cancellationToken);

        if (raised == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        // Old sessions are revoked by a plain UPDATE, not through tracked entities: a session another
        // device is refreshing at this moment would fail the RevokedAt concurrency check and turn the
        // whole change into an error. Here it is simply skipped; the session that refresh creates
        // carries the old version and stops refreshing. The session this change adds is inserted by
        // SaveChanges below, after this UPDATE, so it stays.
        await context.Database.ExecuteSqlAsync($"""
            UPDATE "UserSessions"
            SET "RevokedAt" = {revokedAt}
            WHERE "UserId" = {user.Id} AND "RevokedAt" IS NULL
            """, cancellationToken);

        user.SessionVersion = expectedVersion + 1;
        context.Entry(user).Property(u => u.SessionVersion).IsModified = false;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception) when (
            exception.Entries.Any(entry => entry.Entity is PasswordResetToken))
        {
            // A reset token was consumed after it was read; undo the version write, the revoked
            // sessions and the staged password, and let the handler report an invalid link.
            await transaction.RollbackAsync(cancellationToken);
            context.ChangeTracker.Clear();
            return false;
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public Task SaveAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
