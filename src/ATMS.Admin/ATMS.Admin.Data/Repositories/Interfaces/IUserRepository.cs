using System.Linq.Expressions;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IUserRepository
{
    Task CreateAsync(User user, CancellationToken cancellationToken);

    Task<List<User>> GetAsync();

    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Role>> GetRolesAsync(Guid userId, CancellationToken cancellationToken);
    
    Task<List<Permission>> GetPermissionsAsync(Guid userId, CancellationToken cancellationToken);

    Task<User?> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);

    Task<bool> IsExistAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);

    Task SaveAsync(CancellationToken cancellationToken);
}
