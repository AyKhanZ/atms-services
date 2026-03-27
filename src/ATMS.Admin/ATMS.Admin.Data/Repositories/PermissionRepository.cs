using System.Linq.Expressions;
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class PermissionRepository(AdminDbContext context) : IPermissionRepository
{
    public Task<List<Permission>> GetAsync(CancellationToken cancellationToken)
    {
        return context.Permissions.AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> GetExistingIdsAsync(int[] ids, CancellationToken cancellationToken)
    {
        return await context.Permissions
            .Where(p => ids.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsExistAsync(Expression<Func<Permission, bool>> predicate, CancellationToken cancellationToken)
        => context.Permissions.AnyAsync(predicate, cancellationToken);
}
