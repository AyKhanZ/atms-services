using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Interfaces;
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

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public Task<List<Role>> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        return context.Roles
            .AsNoTracking()
            .Where(r => r.UserRoles.Any(ur => ur.UserId == userId))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(string name, CancellationToken cancellationToken)
        => context.Roles.AnyAsync(r => r.Name == name, cancellationToken);

    public Task UpdateAsync(Role entity, CancellationToken cancellationToken)
    {
        return context.Roles.Where(x => x.Id == entity.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Name, entity.Name)
                .SetProperty(x => x.Description, entity.Description), cancellationToken);
    }
}
