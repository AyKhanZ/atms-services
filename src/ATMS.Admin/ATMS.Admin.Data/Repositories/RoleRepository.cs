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

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<Role>> GetAsync(CancellationToken cancellationToken)
    {
        return context.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(string name, CancellationToken cancellationToken)
        => context.Roles.AnyAsync(r => r.Name == name, cancellationToken);

    public Task<bool> IsExistAsync(Guid id, CancellationToken cancellationToken)
        => context.Roles.AnyAsync(r => r.Id == id, cancellationToken);

    public Task UpdateAsync(Role entity, CancellationToken cancellationToken)
    {
        return context.Roles.Where(x => x.Id == entity.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Name, entity.Name)
                .SetProperty(x => x.Description, entity.Description), cancellationToken);
    }
}
