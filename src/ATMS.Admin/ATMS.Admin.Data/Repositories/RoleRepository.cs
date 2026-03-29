using System.Linq.Expressions;
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class RoleRepository(AdminDbContext context) : IRoleRepository
{
    public async Task CreateAsync(Role entity, CancellationToken cancellationToken)
    {
        await context.Roles.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Roles
            .Where(r => r.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }
    
    public Task<Role?> FindAsync(Expression<Func<Role, bool>> predicate, CancellationToken cancellationToken)
    {
        return context.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<Role?> GetAsync(Expression<Func<Role, bool>> predicate, CancellationToken cancellationToken)
    {
        return await context.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<List<Role>> GetAsync(CancellationToken cancellationToken)
    {
        return context.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(Expression<Func<Role, bool>> predicate, CancellationToken cancellationToken)
        => context.Roles.AnyAsync(predicate, cancellationToken);
    
    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
