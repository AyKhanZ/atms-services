using ATMS.Admin.Data.Entities;

namespace ATMS.Admin.Data.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Role>> GetAsync(CancellationToken cancellationToken);
    Task CreateAsync(Role entity, CancellationToken cancellationToken);
    Task UpdateAsync(Role entity, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> IsExistAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> IsExistAsync(string name, CancellationToken cancellationToken);
}
