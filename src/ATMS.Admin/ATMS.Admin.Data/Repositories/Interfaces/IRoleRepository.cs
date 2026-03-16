using System.Linq.Expressions;
using ATMS.Admin.Data.Entities;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetAsync(Expression<Func<Role, bool>> predicate, CancellationToken cancellationToken);
    
    Task<List<Role>> GetAsync(CancellationToken cancellationToken);
    
    Task CreateAsync(Role entity, CancellationToken cancellationToken);
    
    Task UpdateAsync(Role entity, CancellationToken cancellationToken);
    
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    
    Task<bool> IsExistAsync(Expression<Func<Role, bool>> predicate, CancellationToken cancellationToken);
}
