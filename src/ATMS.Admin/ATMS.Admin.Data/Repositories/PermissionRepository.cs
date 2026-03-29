using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class PermissionRepository(AdminDbContext context) : IPermissionRepository
{
    public Task<List<int>> GetIdsAsync(CancellationToken cancellationToken)
    {
        return context.Permissions
            .AsNoTracking()
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
    }
    
    public Task<List<Permission>> GetAsync(CancellationToken cancellationToken)
    {
        return context.Permissions
            .Include(p => p.Translations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> GetExistingIdsAsync(int[] ids, CancellationToken cancellationToken)
    {
        return await context.Permissions
            .Where(p => ids.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
    }
}
