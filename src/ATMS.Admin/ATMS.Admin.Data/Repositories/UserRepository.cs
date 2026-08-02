using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Data.Criteria;

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


    public Task SaveAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
