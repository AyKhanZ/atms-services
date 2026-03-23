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
}
