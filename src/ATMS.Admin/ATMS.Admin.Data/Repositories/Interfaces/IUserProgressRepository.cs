using System.Linq.Expressions;
using ATMS.Admin.Data.Entities.UserProgresses;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IUserProgressRepository
{
    Task<UserProgress?> GetAsync(Guid userId, CancellationToken cancellationToken);
    
    Task CreateAsync(UserProgress userProgress, CancellationToken cancellationToken);

    Task<UserProgress?> FindAsync(Expression<Func<UserProgress, bool>> predicate, CancellationToken cancellationToken);
    
    Task<bool> IsExistAsync(Expression<Func<UserProgress, bool>> predicate, CancellationToken cancellationToken);
    
    Task SaveAsync(CancellationToken cancellationToken);
}