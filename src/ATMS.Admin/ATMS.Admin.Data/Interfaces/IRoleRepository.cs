using ATMS.Admin.Data.Entities;

namespace ATMS.Admin.Data.Interfaces;

public interface IRoleRepository
{
    Task<bool> IsExistAsync(string name, CancellationToken cancellationToken);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Role>> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task CreateAsync(Role entity, CancellationToken cancellationToken);
    Task UpdateAsync(Role entity, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
