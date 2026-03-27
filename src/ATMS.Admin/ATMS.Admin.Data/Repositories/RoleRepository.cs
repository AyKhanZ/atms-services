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
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<Role?> GetAsync(Expression<Func<Role, bool>> predicate, CancellationToken cancellationToken)
    {
        return await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<List<Role>> GetAsync(CancellationToken cancellationToken)
    {
        return context.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(Expression<Func<Role, bool>> predicate, CancellationToken cancellationToken)
        => context.Roles.AnyAsync(predicate, cancellationToken);

    public Task UpdateAsync(Role entity, CancellationToken cancellationToken)
    {
        return context.Roles.Where(x => x.Id == entity.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Name, entity.Name)
                .SetProperty(x => x.Description, entity.Description), cancellationToken);
    }
    
    public Task SaveAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }
}
