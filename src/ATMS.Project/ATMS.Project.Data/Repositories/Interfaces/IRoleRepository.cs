using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<List<Role>> GetManyAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
}
