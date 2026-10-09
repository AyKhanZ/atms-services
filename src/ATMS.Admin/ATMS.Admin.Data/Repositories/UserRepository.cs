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

public sealed class UserRepository(AdminDbContext context) : IUserRepository
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
        // one UPDATE instead of read + write: postgres locks the row, so parallel wrong passwords are counted one by one
        // only active accounts or ones with an expired timed lock are touched
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

        // no row = a parallel attempt locked it first
        return updated.Count == 0 || updated[0];
    }

    public async Task<bool> TrySavePasswordChangeAsync(
        User user,
        int expectedVersion,
        DateTime revokedAt,
        CancellationToken cancellationToken)
    {
        // version goes up with a conditional UPDATE: a parallel change with the same version matches 0 rows and rolls back
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

        // plain UPDATE, not tracked entities: a session being refreshed right now would fail the concurrency check
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
            // the reset link was used by someone else in the meantime, undo everything
            await transaction.RollbackAsync(cancellationToken);
            context.ChangeTracker.Clear();
            return false;
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public Task SaveAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
