using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class RoleRepository(ProjectDbContext context) : IRoleRepository
{
    public Task<List<Role>> GetManyAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        return context.Roles
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }
}
