using System.Linq.Expressions;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IUserRepository
{
    Task AddAsync(User entity, CancellationToken cancellationToken);

    Task<User?> GetAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);
    
    Task<List<User>> GetAsync(CancellationToken cancellationToken);
    
    Task<User?> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);
    
    
    Task CreateAsync(User entity, CancellationToken cancellationToken);
    
    
    Task<bool> IsExistAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken);
    
    
    Task SaveAsync(CancellationToken cancellationToken);
}
